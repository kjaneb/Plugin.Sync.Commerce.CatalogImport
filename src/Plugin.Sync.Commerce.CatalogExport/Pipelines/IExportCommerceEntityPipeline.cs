using Plugin.Sync.Commerce.CatalogExport.Pipelines.Arguments;
using Sitecore.Commerce.Core;
using Sitecore.Framework.Pipelines;

namespace Plugin.Sync.Commerce.CatalogExport.Pipelines
{
    [PipelineDisplayName("IExportCommerceEntityPipeline")]
    public interface IExportCommerceEntityPipeline : IPipeline<ExportCommerceEntityArgument, ExportCommerceEntityArgument, CommercePipelineExecutionContext>, 
        IPipelineBlock<ExportCommerceEntityArgument, ExportCommerceEntityArgument, CommercePipelineExecutionContext>, IPipelineBlock, IPipeline
    {
    }
}