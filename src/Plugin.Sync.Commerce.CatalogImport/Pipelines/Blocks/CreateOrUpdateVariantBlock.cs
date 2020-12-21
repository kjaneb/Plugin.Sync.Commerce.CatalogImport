using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Plugin.Sync.Commerce.CatalogImport.Extensions;
using Plugin.Sync.Commerce.CatalogImport.Models;
using Plugin.Sync.Commerce.CatalogImport.Pipelines.Arguments;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.Core.Commands;
using Sitecore.Commerce.Plugin.Catalog;
using Sitecore.Commerce.Plugin.Composer;
using Sitecore.Commerce.Plugin.Pricing;
using Sitecore.Framework.Conditions;
using Sitecore.Framework.Pipelines;

namespace Plugin.Sync.Commerce.CatalogImport.Pipelines.Blocks
{
    public class CreateOrUpdateVariantBlock : AsyncPipelineBlock<ImportCatalogEntityArgument, ImportCatalogEntityArgument, CommercePipelineExecutionContext>
    {
        private readonly CommerceCommander _commerceCommander;
        private readonly CommerceEntityImportHelper _importHelper;

        public CreateOrUpdateVariantBlock(CommerceCommander commerceCommander, ComposerCommander composerCommander)
        {
            _commerceCommander = commerceCommander;
            _importHelper = new CommerceEntityImportHelper(commerceCommander, composerCommander);
        }

        public override async Task<ImportCatalogEntityArgument> RunAsync(ImportCatalogEntityArgument arg, CommercePipelineExecutionContext context)
        {
            var entityData = context.GetModel<CatalogEntityDataModel>();

            Condition.Requires(entityData, "CatalogEntityDataModel is required to exist in order for CommercePipelineExecutionContext to run").IsNotNull();
            Condition.Requires(entityData.EntityId, "EntityId is reguired in input JSON data").IsNotNullOrEmpty();
            Condition.Requires(entityData.EntityName, "EntityName is reguired in input JSON data").IsNotNullOrEmpty();
            Condition.Requires(entityData.ParentCatalogName, "ParentCatalogName Name is reguired to be present in input JSON data or set default in SellabeItemMappingPolicy").IsNotNullOrEmpty();

            var sellableItem =  await _commerceCommander.Command<FindEntityCommand>().Process(context.CommerceContext, typeof(SellableItem), entityData.CommerceEntityId) as SellableItem;
            if (sellableItem != null)
            {
                await AddVariant(entityData, sellableItem, context);
            }

            return arg;
        }

        private async Task AddVariant(CatalogEntityDataModel variantData, SellableItem sellableItem, CommercePipelineExecutionContext context)
        {
            var displayName = variantData.EntityFields.ContainsKey("DisplayName")
                ? variantData.EntityFields["DisplayName"]
                : variantData.EntityName;

            //Create the new variant.
            var variantItem = await _commerceCommander.Command<CreateSellableItemVariationCommand>()
                .Process(context.CommerceContext, sellableItem.Id, variantData.EntityId, variantData.EntityName, displayName);

            if (variantItem != null)
            {
                sellableItem = variantItem;
            }

            //Get the new variant from the sellable item.
            var variation = sellableItem.GetVariation(variantData.EntityId);
            if (variation == null)
            {
                context.Logger.LogError($"Could not find variant {variantData.EntityId} in sellable item {sellableItem.Id}");
                return;
            }

            //Add a list price component to the sellable item.
            //Create more of these for any other components or custom components to be added.
            AddListPrice(variantData, variation);
            //Update the display properties component.

            //Save the changes
            await _commerceCommander.PersistEntity(context.CommerceContext, sellableItem);
        }

        /// <summary>
        /// Add a list price to a variant.
        /// </summary>
        /// <param name="variantData">The bootstrap variant with the price data.</param>
        /// <param name="variant">The variant component to be updated.</param>
        private void AddListPrice(CatalogEntityDataModel variantData, ItemVariationComponent variant)
        {
            var listPriceString = variantData.EntityFields.ContainsKey("ListPrice") ? variantData.EntityFields["ListPrice"] : "";

            var listPrice = 0M;

            if (Decimal.TryParse(listPriceString, out listPrice))
            {
                if (listPrice == 0)
                {
                    return;
                }

                var listPricingPolicy = variant.GetPolicy<ListPricingPolicy>();
                listPricingPolicy.AddPrice(new Money("USD", listPrice));
            }
        }
    }
}
