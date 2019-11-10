// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ConfigureSitecore.cs" company="Sitecore Corporation">
//   Copyright (c) Sitecore Corporation 1999-2017
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using Plugin.Sync.Commerce.CatalogImport.Pipelines;
using Plugin.Sync.Commerce.CatalogImport.Pipelines.Blocks;

namespace Plugin.Sync.Commerce.CatalogImport
{
    using System.Reflection;
    using Microsoft.Extensions.DependencyInjection;
    using Plugin.Sync.Commerce.CatalogImport.Pipelines.ViewBlocks;
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
                 .ConfigurePipeline<IGetEntityViewPipeline>(
                    configure =>
                   {
                       configure.Add<GetCustomImagesViewBlock>().After<GetSellableItemDetailsViewBlock>();
                   })
                 .ConfigurePipeline<IPopulateEntityViewActionsPipeline>(
                    configure =>
                    {
                        configure.Add<PopulateCustomImagesActionBlock>().After<InitializeEntityViewActionsBlock>();
                    })
                 .ConfigurePipeline<IDoActionPipeline>(
                    configure =>
                    {
                        configure.Add<DoActionEditCustomImagesBlock>().After<ValidateEntityVersionBlock>();
                    })
                 .ConfigurePipeline<IGetEntityViewPipeline>(
                    configure =>
                    {
                        configure.Add<GetSellableItemStatusViewBlock>().After<GetSellableItemDetailsViewBlock>();
                    })
                ); 

            services.RegisterAllCommands(assembly);
        }
    }
}