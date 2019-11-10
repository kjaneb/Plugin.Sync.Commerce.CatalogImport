using Microsoft.Extensions.Logging;
using Plugin.Sync.Commerce.CatalogExport.Pipelines.Arguments;
using Sitecore.Commerce.Core;
using Sitecore.Framework.Pipelines;

namespace Plugin.Sync.Commerce.CatalogExport.Pipelines
{
    public class ExportProductMinionPipline : CommercePipeline<CatalogExportArgument, CatalogExportArgument>, 
        IExportProductMinionPipeline, IPipeline<CatalogExportArgument, CatalogExportArgument, CommercePipelineExecutionContext>, 
        IPipelineBlock<CatalogExportArgument, CatalogExportArgument, CommercePipelineExecutionContext>, IPipelineBlock, IPipeline
    {
        public ExportProductMinionPipline(IPipelineConfiguration<IExportProductMinionPipeline> configuration, ILoggerFactory loggerFactory) 
            : base((IPipelineConfiguration)configuration, loggerFactory)
        {
        }
    }
}

  