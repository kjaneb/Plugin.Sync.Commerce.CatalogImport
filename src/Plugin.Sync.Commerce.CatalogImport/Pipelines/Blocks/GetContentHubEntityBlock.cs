using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Plugin.Sync.Commerce.CatalogImport.Pipelines.Arguments;
using Plugin.Sync.Commerce.CatalogImport.Policies;
using Serilog;
using Sitecore.Commerce.Core;
using Sitecore.Framework.Caching;
using Sitecore.Framework.Pipelines;
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace Plugin.Sync.Commerce.CatalogImport.Pipelines.Blocks
{
    /// <summary>
    /// Retrieve Content Hub entity via its API
    /// TODO: refactor this to use CH nuget instead of direct HTTP REST calls
    /// </summary>
    [PipelineDisplayName("GetContentHubEntityBlock")]
    public class GetContentHubEntityBlock : PipelineBlock<ImportCatalogEntityArgument, ImportCatalogEntityArgument, CommercePipelineExecutionContext>
    {
        #region Private fields
        ICacheManager _cacheManager;
        const string TOKEN_NAME = "ContentHubSecurityToken";
        #endregion

        #region Public methods
        /// <summary>
        /// Public contructor
        /// </summary>
        /// <param name="commerceCommander"></param>
        /// <param name="composerCommander"></param>
        /// <param name="importHelper"></param>
        public GetContentHubEntityBlock(ICacheManager cacheManager)
        {
            _cacheManager = cacheManager;
        }

        /// <summary>
        /// Main execution point
        /// </summary>
        /// <param name="arg"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public override async Task<ImportCatalogEntityArgument> Run(ImportCatalogEntityArgument arg, CommercePipelineExecutionContext context)
        {
            try
            {
                var contentHubPolicy = context.GetPolicy<ContentHubConnectionPolicy>();
                var entityObject = await GetEntity(arg.ContentHubEntityId, contentHubPolicy).ConfigureAwait(false);
                if (entityObject != null)
                {
                    arg.Request = entityObject;
                }
                return arg;
            }
            catch (Exception ex)
            {
                var errorMessage = $"Error retrieving Content Hub Entity {arg.ContentHubEntityId}.";
                Log.Error(ex, errorMessage);
                context.Abort(errorMessage, ex);
                return arg;
            }
        }

        #endregion

        #region Private methods
        private async Task<JObject> GetEntity(string entityId, ContentHubConnectionPolicy contentHubPolicy)
        {
            var request = WebRequest.Create($"{contentHubPolicy.ProtocolAndHost}/api/entities/{entityId}");
            request.Method = "GET";
            var token = await GetToken(contentHubPolicy).ConfigureAwait(false);
            request.Headers.Add("X-Auth-Token", token);

            string responseContent = null;
            using (var response = request.GetResponse())
            {
                using (var responseStream = response.GetResponseStream())
                {
                    using (var streamReader = new StreamReader(responseStream))
                    {
                        responseContent = streamReader.ReadToEnd();
                    }
                }
            }
            if (!string.IsNullOrEmpty(responseContent))
            {
                return JObject.Parse(responseContent);
            }

            return null;
        }

        /// <summary>
        /// Get security token to use for Content Hub API calls
        /// </summary>
        /// <param name="contentHubPolicy"></param>
        /// <returns></returns>
        private async Task<string> GetToken(ContentHubConnectionPolicy contentHubPolicy)
        {
            var cache = _cacheManager.GetCache(contentHubPolicy.TokenCacheName);
            if (cache == null)
            {
                cache = _cacheManager.CreateCache(contentHubPolicy.TokenCacheName);
            }

            var securityToken = await cache.Get<string>(TOKEN_NAME).ConfigureAwait(false);
            if (string.IsNullOrEmpty(securityToken))
            {
                var request = WebRequest.Create(string.Format("{0}/api/authenticate", contentHubPolicy.ProtocolAndHost));
                request.Method = "POST";
                request.ContentType = "application/json";

                using (var streamWriter = new StreamWriter(request.GetRequestStream()))
                {
                    var jsonObject = new
                    {
                        user_name = contentHubPolicy.UserName,
                        password = contentHubPolicy.Password,
                        discard_existing = false
                    };

                    streamWriter.Write(JsonConvert.SerializeObject(jsonObject));
                    streamWriter.Flush();
                    streamWriter.Close();
                }

                string responseContent = null;
                using (var response = request.GetResponse())
                {
                    using (var responseStream = response.GetResponseStream())
                    {
                        using (var streamReader = new StreamReader(responseStream))
                        {
                            responseContent = streamReader.ReadToEnd();
                        }
                    }
                }

                var o = JObject.Parse(responseContent);
                securityToken = (string)o["token"];

                await cache.SetString(TOKEN_NAME, securityToken).ConfigureAwait(false);
            }

            return securityToken;
        }
        #endregion
    }
}