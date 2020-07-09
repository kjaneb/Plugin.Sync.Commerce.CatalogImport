using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Plugin.Sync.Commerce.CatalogImport.Extensions;
using Plugin.Sync.Commerce.CatalogImport.Helpers;
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
    [PipelineDisplayName("GetRelatedContentHubEntitiesBlock")]
    public class GetRelatedContentHubAssetsPublicLinksBlock : PipelineBlock<ImportCatalogEntityArgument, ImportCatalogEntityArgument, CommercePipelineExecutionContext>
    {
        #region Private fields
        private ContentHubHelper _contentHubHelper;
        #endregion

        #region Public methods
        /// <summary>
        /// Public contructor
        /// </summary>
        /// <param name="commerceCommander"></param>
        /// <param name="composerCommander"></param>
        /// <param name="importHelper"></param>
        public GetRelatedContentHubAssetsPublicLinksBlock(ICacheManager cacheManager)
        {
            _contentHubHelper = new ContentHubHelper(cacheManager);
        }

        /// <summary>
        /// Main execution point
        /// </summary>
        /// <param name="arg"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public override async Task<ImportCatalogEntityArgument> Run(ImportCatalogEntityArgument arg, CommercePipelineExecutionContext context)
        {
            if (arg?.Entity == null)
            {
                return arg;
            }

            try
            {
                var contentHubPolicy = context.GetPolicy<ContentHubConnectionPolicy>();
                var entityObject = await _contentHubHelper.GetEntityById(arg.ContentHubEntityId, contentHubPolicy).ConfigureAwait(false);
                if (entityObject != null)
                {
                    arg.Entity = entityObject;
                    if (arg.MappingPolicy != null && !string.IsNullOrEmpty(arg.MappingPolicy.ParentRelationEntityPath))
                    {
                        var parentRelationUrl = arg.Entity.SelectValue<string>(arg.MappingPolicy.ParentRelationEntityPath);
                        if (!string.IsNullOrEmpty(parentRelationUrl))
                        {
                            var parentRelationsEntity = await _contentHubHelper.GetEntityByUrl(parentRelationUrl, contentHubPolicy).ConfigureAwait(false);
                            if (parentRelationsEntity != null)
                            {
                                arg.ParentRelationsEntity = parentRelationsEntity;
                            } 
                        }
                    }
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
       
        #endregion
    }
}