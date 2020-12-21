using Microsoft.Extensions.Logging;
using Plugin.Sync.Commerce.CatalogImport.Entities;
using Plugin.Sync.Commerce.CatalogImport.Pipelines.Arguments;
using Sitecore.Commerce.Core;
using Sitecore.Framework.Pipelines;

namespace Plugin.Sync.Commerce.CatalogImport.Pipelines
{
    public class ImportSellableItemVariantPipeline :  CommercePipeline<ImportCatalogEntityArgument, ImportCatalogEntityArgument>, IImportSellableItemVariantPipeline
    {
        public ImportSellableItemVariantPipeline(IPipelineConfiguration<IImportSellableItemVariantPipeline> configuration, ILoggerFactory loggerFactory) : base(configuration, loggerFactory)
        {
        }
    }
}