using Plugin.Sync.Commerce.CatalogImport.Entities;
using Plugin.Sync.Commerce.CatalogImport.Pipelines.Arguments;
using Sitecore.Commerce.Core;
using Sitecore.Framework.Pipelines;

namespace Plugin.Sync.Commerce.CatalogImport.Pipelines
{
    [PipelineDisplayName("ImportItemDefinitionPipeline")]
    public interface IImportItemDefinitionPipeline : IPipeline<ImportItemDefinitionArgument, ImportItemDefinitionArgument, CommercePipelineExecutionContext>
    {
    }
}