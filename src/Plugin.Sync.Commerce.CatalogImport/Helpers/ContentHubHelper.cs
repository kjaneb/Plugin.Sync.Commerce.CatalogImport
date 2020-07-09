using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Plugin.Sync.Commerce.CatalogImport.Policies;
using Sitecore.Diagnostics;
using Sitecore.Framework.Caching;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Plugin.Sync.Commerce.CatalogImport.Helpers
{
    public class ContentHubHelper
    {
        ICacheManager _cacheManager;
        const string TOKEN_NAME = "ContentHubSecurityToken";
        public ContentHubHelper(ICacheManager cacheManager)
        {
            _cacheManager = cacheManager;
        }

        public async Task<JObject> GetEntityByUrl(string entityUrl, ContentHubConnectionPolicy contentHubPolicy)
        {
            var request = WebRequest.Create(entityUrl);
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

        public async Task<JObject> GetEntityById(string entityId, ContentHubConnectionPolicy contentHubPolicy)
        {
            var entityUrl = $"{contentHubPolicy.ProtocolAndHost}/api/entities/{entityId}";
            return await GetEntityByUrl(entityUrl, contentHubPolicy);
        }

        /// <summary>
        /// Get security token to use for Content Hub API calls
        /// </summary>
        /// <param name="contentHubPolicy"></param>
        /// <returns></returns>
        public async Task<string> GetToken(ContentHubConnectionPolicy contentHubPolicy)
        {
            try
            {
                var cache = _cacheManager.GetCache(contentHubPolicy.TokenCacheName);
                if (cache == null)
                {
                    cache = _cacheManager.CreateCache(contentHubPolicy.TokenCacheName);
                }

                string securityToken = await cache.Get<string>(TOKEN_NAME).ConfigureAwait(false);
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

                    var cacheEntryOptions = new CacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(3000)
                    };
                    await cache.SetString(TOKEN_NAME, securityToken, cacheEntryOptions).ConfigureAwait(false);
                }
                //Log.Warn($"retrieved CH token: {securityToken}", this);
                return securityToken;
            }
            catch (Exception ex)
            {
                Log.Error("Error retrieving Content Hub token", ex, this);
                throw;
            }
        }
    }
}
