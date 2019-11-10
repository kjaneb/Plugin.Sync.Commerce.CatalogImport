using Microsoft.Extensions.Logging;
using Plugin.Sync.Commerce.CatalogImport.Entities;
using Plugin.Sync.Commerce.CatalogImport.Pipelines.Arguments;
using Sitecore.Commerce.Core;
using Sitecore.Framework.Pipelines;

namespace Plugin.Sync.Commerce.CatalogImport.Pipelines
{
    public class ImportCategoryPipeline : CommercePipeline<ImportCommerceEntityArgument, ImportCommerceEntityResponse>, IImportCategoryPipeline
    {
        //https://sitecore.stackexchange.com/questions/11035/unable-to-resolve-service-for-type-ipipelineconfiguration-while-attempting-to
        public ImportCategoryPipeline(IPipelineConfiguration<IImportCategoryPipeline> configuration, ILoggerFactory loggerFactory) : base(configuration, loggerFactory)
        {
        }
    }
}