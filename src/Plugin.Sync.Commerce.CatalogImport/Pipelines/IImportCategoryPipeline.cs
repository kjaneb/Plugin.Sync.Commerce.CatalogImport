using Plugin.Sync.Commerce.CatalogImport.Entities;
using Plugin.Sync.Commerce.CatalogImport.Pipelines.Arguments;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.Plugin.Catalog;
using Sitecore.Framework.Pipelines;

namespace Plugin.Sync.Commerce.CatalogImport.Pipelines
{
    [PipelineDisplayName("ImportCategoryPipeline")]
    public interface IImportCategoryPipeline : IPipeline<ImportCategoryArgument, ImportCategoryResponse, CommercePipelineExecutionContext>
    {
        
    }
}