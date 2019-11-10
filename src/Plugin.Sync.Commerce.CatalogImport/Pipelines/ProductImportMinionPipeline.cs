using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Plugin.Sync.Commerce.CatalogImport.Pipelines.Arguments;
using Sitecore.Commerce.Core;
using Sitecore.Framework.Pipelines;

namespace Plugin.Sync.Commerce.CatalogImport.Pipelines
{
    public class ProductImportMinionPipeline : CommercePipeline<ProductImportMinionArgument, MinionRunResultsModel>, IProductImportMinionPipeline, IPipeline<ProductImportMinionArgument, MinionRunResultsModel, CommercePipelineExecutionContext>, IPipelineBlock<ProductImportMinionArgument, MinionRunResultsModel, CommercePipelineExecutionContext>, IPipelineBlock, IPipeline
    {
        public ProductImportMinionPipeline(IPipelineConfiguration<IProductImportMinionPipeline> configuration, ILoggerFactory loggerFactory) : base((IPipelineConfiguration)configuration, loggerFactory)
        {
        }
    }
}

  