using Plugin.Sync.Commerce.CatalogImport.Pipelines.Arguments;
using Sitecore.Commerce.Core;
using Sitecore.Framework.Pipelines;

namespace Plugin.Sync.Commerce.CatalogImport.Pipelines
{
    [PipelineDisplayName("IProductImportMinionPipeline")]
    public interface IProductImportMinionPipeline : IPipeline<ProductImportMinionArgument, MinionRunResultsModel, CommercePipelineExecutionContext>, IPipelineBlock<ProductImportMinionArgument, MinionRunResultsModel, CommercePipelineExecutionContext>, IPipelineBlock, IPipeline
    {
    }
}