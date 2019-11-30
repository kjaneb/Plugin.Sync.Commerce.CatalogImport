using Plugin.Ryder.Commerce.CatalogExport.Util;
using Plugin.Sync.Commerce.CatalogExport.Pipelines.Arguments;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.Plugin.Management;
using Sitecore.Framework.Pipelines;
using System;
using System.Threading.Tasks;

namespace Plugin.Sync.Commerce.CatalogExport.Pipelines.Blocks
{
    /// <summary>
    /// Gets "Message template" content item from Sitcore and saves it in current pipeline as  PropertiesModel name-value collection
    /// </summary>
    public class GetTemplateFromSitecoreBlock : PipelineBlock<ExportCommerceEntityArgument, ExportCommerceEntityArgument, CommercePipelineExecutionContext>
    {
        //IGetItemByIdPipeline _getItemByIdPipeline;
        IGetItemByPathPipeline _getItemByPathPipeline;
        public GetTemplateFromSitecoreBlock(IGetItemByPathPipeline getItemByPathPipeline/*, IGetItemByIdPipeline getItemByIdPipeline*/)
        {
            _getItemByPathPipeline = getItemByPathPipeline;
        }

#pragma warning disable 1998
        public override async Task<ExportCommerceEntityArgument> Run(ExportCommerceEntityArgument arg, CommercePipelineExecutionContext context)
        {
            if (!arg.TemplateLocation.Equals("sitecore", StringComparison.OrdinalIgnoreCase))
                return arg;

            var language = context.CommerceContext.CurrentLanguage();
            if (language == null)
            {
                arg.EntityNotFound = true;
                return context.AbortPipeline(arg, $"Current Language canot be idetified - check language configuration in Commerce Control Panel. {this.GetType().Name}. Request EntityId={arg.EntityId}.");
            }

            var itemInfo = arg.TemplatePath.Split('|');
            if (itemInfo.Length < 2)
            {
                return context.AbortPipeline(arg, $"Sitecore field name portion of the View path is missing in 'view' parameter. For views residing in Sitecore content tree the 'view' parameter must have Sitecore item path followed by | followed by field name, e.g. 'sitecore item path'|'field name' : view path. Block name: {this.GetType().Name}. Request EntityId={arg.EntityId}.");
            }

            var itemModelArgument = new ItemModelArgument(itemInfo[0])
            {
                Language = language,
            };

            var itemModel = await _getItemByPathPipeline.Run(itemModelArgument, context);

            if (itemModel == null)
            {
                arg.EntityNotFound = true;
                return context.AbortPipeline(arg, $"Sitecore item with Razor template not found in Sitecore.Item Path: {itemInfo[0]}. {this.GetType().Name}. Entity ID={arg.EntityId} not found.");
            }

            if (!itemModel.ContainsKey(itemInfo[1]))
            {
                arg.EntityNotFound = true;
                return context.AbortPipeline(arg, $"Field named '{itemInfo[1]}' not found on Item '{itemInfo[0]}'. {this.GetType().Name}. Entity ID={arg.EntityId} not found.");
            }

            arg.ViewTemplate = (string)itemModel[itemInfo[1]];
            return arg;

        }
#pragma warning restore 1998
    }
}
