using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.Plugin.Management;
using Sitecore.Data.Items;
using Sitecore.Services.Core.Model;

namespace Plugin.Sync.Commerce.CatalogImport.Pipelines.Helper
{
    public class SitecoreContentReader
    {
        private readonly IGetItemByPathPipeline _getItemByPathPipeline;
        private readonly IGetItemChildrenPipeline _getItemChildrenPipeline;

        public SitecoreContentReader(IGetItemByPathPipeline getItemByPathPipeline, IGetItemChildrenPipeline getItemChildrenPipeline)
        {
            _getItemByPathPipeline = getItemByPathPipeline;
            _getItemChildrenPipeline = getItemChildrenPipeline;
        }
        public ItemModel GetSitecoreItem(CommercePipelineExecutionContext context, string sitecoreContentRootPath)
        {
            var mappingArgument = new ItemModelArgument(sitecoreContentRootPath) { Language = context.CommerceContext.CurrentLanguage() };
           
            var taskResult = Task.Run<ItemModel>(async () => await _getItemByPathPipeline.Run(mappingArgument,context));
            return taskResult.Result;
        }

        public IEnumerable<ItemModel> GetSitecoreChildrenItems(CommercePipelineExecutionContext context, string itemGuid)
        {
            var mappingArgument = new ItemModelArgument(itemGuid) { Language = context.CommerceContext.CurrentLanguage()};
            var taskResult = Task.Run<IEnumerable<ItemModel>>(async () => await _getItemChildrenPipeline.Run(mappingArgument, context));
            return taskResult.Result;
        }
    }
}