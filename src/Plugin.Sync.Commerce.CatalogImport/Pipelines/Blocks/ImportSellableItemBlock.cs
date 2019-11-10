using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Plugin.Sync.Commerce.CatalogImport.Entities;
using Plugin.Sync.Commerce.CatalogImport.Extensions;
using Plugin.Sync.Commerce.CatalogImport.Helpers;
using Plugin.Sync.Commerce.CatalogImport.Models;
using Plugin.Sync.Commerce.CatalogImport.Pipelines.Arguments;
using Plugin.Sync.Commerce.CatalogImport.Pipelines.Helper;
using Plugin.Sync.Commerce.CatalogImport.Policies;
using Serilog;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.Core.Commands;
using Sitecore.Commerce.EntityViews;
using Sitecore.Commerce.EntityViews.Commands;
using Sitecore.Commerce.Plugin.Catalog;
using Sitecore.Commerce.Plugin.Composer;
using Sitecore.Commerce.Plugin.Management;
using Sitecore.Commerce.Plugin.Pricing;
using Sitecore.Commerce.Plugin.Views;
using Sitecore.Commerce.Plugin.Workflow;
using Sitecore.Framework.Conditions;
using Sitecore.Framework.Pipelines;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Plugin.Sync.Commerce.CatalogImport.Pipelines.Blocks
{
    [PipelineDisplayName("SyncSellableItemBlock")]
    public class ImportSellableItemBlock : PipelineBlock<ImportSellableItemArgument, SellableItemResponse, CommercePipelineExecutionContext>
    {
        private readonly CommerceCommander _commerceCommander;
        private readonly ComposerCommander _composerCommander;
        private readonly FindEntityCommand _findEntityCommand;
        private readonly CreateSellableItemCommand _createSellableItemCommand;

        private readonly GetEntityViewCommand _getEntityViewCommand;
        private readonly DoActionCommand _doActionCommand;
        private SitecoreContentReader _sitecoreContentReader;
        private readonly IRemoveListEntitiesPipeline _removeListEntitiesPipeline;
        private readonly IAddListEntitiesPipeline _addListEntitiesPipeline;
        private readonly IFindEntitiesInListPipeline _findEntitiesInListPipeline;
        private readonly CommerceEntityImportHelper _importHelper;

        public ImportSellableItemBlock(CommerceCommander commerceCommander, FindEntityCommand findEntityCommand, CreateSellableItemCommand createSellableItemCommand,
            GetEntityViewCommand getEntityViewCommand, DoActionCommand doActionCommand, IGetItemByPathPipeline getItemByPathPipeline,
            IGetItemChildrenPipeline getItemChildrenPipeline, ComposerCommander composerCommander,
            IRemoveListEntitiesPipeline removeListEntitiesPipeline, IAddListEntitiesPipeline addListEntitiesPipeline, IFindEntitiesInListPipeline findEntitiesInListPipeline,
            CommerceEntityImportHelper importHelper
            )
        {
            _commerceCommander = commerceCommander;
            _composerCommander = composerCommander;
            _findEntityCommand = findEntityCommand;
            _createSellableItemCommand = createSellableItemCommand;
            _getEntityViewCommand = getEntityViewCommand;
            _doActionCommand = doActionCommand;
            _removeListEntitiesPipeline = removeListEntitiesPipeline;
            _addListEntitiesPipeline = addListEntitiesPipeline;
            _findEntitiesInListPipeline = findEntitiesInListPipeline;
            _importHelper = importHelper;
            _sitecoreContentReader = new SitecoreContentReader(getItemByPathPipeline, getItemChildrenPipeline);
        }
        public override async Task<SellableItemResponse> Run(ImportSellableItemArgument arg, CommercePipelineExecutionContext context)
        {

            var response = await SyncCatalogItemData(arg, context);
            return response;
        }

        public async Task<SellableItemResponse> SyncCatalogItemData(ImportSellableItemArgument arg, CommercePipelineExecutionContext context)
        {
            var mappingPolicy = context.CommerceContext.GetPolicy<SellableItemMappingPolicy>();
            Condition.Requires(mappingPolicy, nameof(mappingPolicy)).IsNotNull();
            Condition.Requires(arg.JsonData, nameof(arg.JsonData)).IsNotNull();
            Condition.Requires(mappingPolicy.IdPath, nameof(mappingPolicy.IdPath)).IsNotNullOrEmpty();
            Condition.Requires(mappingPolicy.NamePath, nameof(mappingPolicy.NamePath)).IsNotNullOrEmpty();
            //Condition.Requires(mappingPolicy.CatalogNamePath, nameof(mappingPolicy.CatalogNamePath)).IsNotNullOrEmpty();

            var itemId = arg.JsonData.SelectValue<string>(mappingPolicy.IdPath);
            Condition.Requires(itemId, "Item ID is reguired in input JSON data").IsNotNullOrEmpty();

            //Get Catalog
            var catalogName = arg.JsonData.SelectValue<string>(mappingPolicy.CatalogNamePath);
            if (string.IsNullOrEmpty(catalogName) && !string.IsNullOrEmpty(mappingPolicy.DefaultCatalogName))
            {
                catalogName = mappingPolicy.DefaultCatalogName;
            }
            Condition.Requires(catalogName, "Catalog Name must be present in input JSON data or set in SellableItemMappingPolicy").IsNotNullOrEmpty();
            _importHelper.AssetCatalogExists(context, catalogName);

            //Get or create sellable item
            var commerceEntityId = $"{CommerceEntity.IdPrefix<SellableItem>()}{itemId}";
            var sellableItem = await _findEntityCommand.Process(context.CommerceContext, typeof(SellableItem), commerceEntityId) as SellableItem;
            if (sellableItem == null)
            {
                var name = arg.JsonData.SelectValue<string>(mappingPolicy.NamePath) ?? itemId;
                var displayName = arg.JsonData.SelectValue<string>(mappingPolicy.DisplayNamePath) ?? name;
                var description = arg.JsonData.SelectValue<string>(mappingPolicy.DescriptionPath);

                Condition.Requires(itemId, "Item ID is reguired in input JSON data").IsNotNullOrEmpty();
                await _createSellableItemCommand.Process(context.CommerceContext, itemId, name, displayName, description);
                sellableItem = await _findEntityCommand.Process(context.CommerceContext, typeof(SellableItem), commerceEntityId) as SellableItem;
            }

            //Associate catalog and category
            //TODO: allow multiple categories
            var categoryName = arg.JsonData.SelectValue<string>(mappingPolicy.ParentCategoryNamePath);
            if (string.IsNullOrEmpty(categoryName) && !string.IsNullOrEmpty(mappingPolicy.DefaultCategoryName))
            {
                categoryName = mappingPolicy.DefaultCategoryName;
            }

            await _importHelper.AssociateWithParentCategory(context, catalogName, sellableItem, categoryName);

            sellableItem = await _importHelper.SyncChildViews(sellableItem, arg.JsonData, mappingPolicy, context.CommerceContext) as SellableItem;

            return new SellableItemResponse
            {
                SellableItem = sellableItem
            };

        }

        

    }
}