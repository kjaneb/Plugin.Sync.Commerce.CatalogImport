using Plugin.Sync.Commerce.CatalogExport.Pipelines.Arguments;
using Sitecore.Commerce.Core;
using Sitecore.Framework.Pipelines;

namespace Plugin.Sync.Commerce.CatalogExport.Pipelines
{
    [PipelineDisplayName("IExportProductMinionPipeline")]
    public interface IExportProductMinionPipeline : IPipeline<CatalogExportArgument, CatalogExportArgument, CommercePipelineExecutionContext>, 
        IPipelineBlock<CatalogExportArgument, CatalogExportArgument, CommercePipelineExecutionContext>, IPipelineBlock, IPipeline
    {
    }
}