using Plugin.Sync.Commerce.CatalogImport.Pipelines;
using Plugin.Sync.Commerce.CatalogImport.Pipelines.Blocks;

namespace Plugin.Sync.Commerce.CatalogImport
{
    using System.Reflection;
    using Microsoft.Extensions.DependencyInjection;
    using Sitecore.Commerce.Core;
    using Sitecore.Commerce.EntityViews;
    using Sitecore.Commerce.Plugin.Catalog;
    using Sitecore.Commerce.Plugin.SQL;
    using Sitecore.Framework.Configuration;
    using Sitecore.Framework.Pipelines.Definitions.Extensions;

    public class ConfigureSitecore : IConfigureSitecore
    {
        public void ConfigureServices(IServiceCollection services)
        {
            var assembly = Assembly.GetExecutingAssembly();
            services.RegisterAllPipelineBlocks(assembly);

            services.Sitecore().Pipelines(config => config
                .ConfigurePipeline<IConfigureServiceApiPipeline>(configure => configure.Add<ConfigureServiceApiBlock>())
                .AddPipeline<IImportCategoryPipeline, ImportCategoryPipeline>(
                    configure =>
                    {
                        configure.Add<ImportCategoryBlock>();
                    })
                .AddPipeline<IImportSellableItemPipeline, ImportSellableItemPipeline>(
                    configure =>
                    {
                        configure.Add<ImportSellableItemBlock>();
                    })
                 .ConfigurePipeline<IPersistEntityPipeline>(
                    configure =>
                    {
                        configure.Add<AddSellableItemToUpdatedSellableItemsListBlock>().Before<PersistEntityBlock>();
                    })
                ); 

            services.RegisterAllCommands(assembly);
        }
    }
}