using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Plugin.Sync.Commerce.CatalogImport.Entities;
using Plugin.Sync.Commerce.CatalogImport.Extensions;
using Plugin.Sync.Commerce.CatalogImport.Models;
using Plugin.Sync.Commerce.CatalogImport.Pipelines.Arguments;
using Plugin.Sync.Commerce.CatalogImport.Policies;
using Serilog;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.Core.Commands;
using Sitecore.Commerce.Plugin.Catalog;
using Sitecore.Commerce.Plugin.Composer;
using Sitecore.Framework.Conditions;
using Sitecore.Framework.Pipelines;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Plugin.Sync.Commerce.CatalogImport.Pipelines.Blocks
{
    /// <summary>
    /// Import data into an existing SellableItem or new SellableItem entity
    /// </summary>
    [PipelineDisplayName("SetSellableItemEntityFieldsBlock")]
    public class SetSellableItemEntityFieldsBlock : PipelineBlock<ImportSellableItemArgument, ImportSellableItemResponse, CommercePipelineExecutionContext>
    {
        #region Private fields
        private readonly CommerceCommander _commerceCommander;
        private readonly CommerceEntityImportHelper _importHelper;
        #endregion

        #region Public methods
        /// <summary>
        /// Public contructor
        /// </summary>
        /// <param name="commerceCommander"></param>
        /// <param name="composerCommander"></param>
        /// <param name="importHelper"></param>
        public SetSellableItemEntityFieldsBlock(CommerceCommander commerceCommander, ComposerCommander composerCommander)
        {
            _commerceCommander = commerceCommander;
            _importHelper = new CommerceEntityImportHelper(commerceCommander, composerCommander);
        }

        /// <summary>
        /// Main execution point
        /// </summary>
        /// <param name="arg"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public override async Task<ImportSellableItemResponse> Run(ImportSellableItemArgument arg, CommercePipelineExecutionContext context)
        {
            //TODO: add an option to only import data if SellableItem already exists (don't create a new one)
            //TODO: add an option to only import data if SellableItem don't exist (don't update existing ones)
            //_importHelper.AssertRootFields(arg, mappingPolicy);

            //var itemId = arg.JsonData.SelectValue<string>(mappingPolicy.IdPath);
            //Condition.Requires(itemId, "Item ID is reguired in input JSON data").IsNotNullOrEmpty();

            Condition.Requires(arg.EntityData.Id, "Item ID is reguired in input JSON data").IsNotNullOrEmpty();
            Condition.Requires(arg.EntityData.CatalogName, "Catalog Name is reguired to be present in input JSON data or set default in SellabeItemMappingPolicy").IsNotNullOrEmpty();

            //Get or create sellable item
            arg.SellableItem = await GetOrCreateSellableItem(arg.EntityData, context);
            //Associate catalog and category
            await AssociateSellableItemWithParentEntities(arg.EntityData.CatalogName, arg.EntityData.ParentCategoryName, arg.SellableItem, context.CommerceContext);

            //Import SellableIltem field values
            //sellableItem.Get
            //sellableItem.View

            //sellableItem = await _importHelper.ImportComposerViewsFields(sellableItem, arg.JsonData, mappingPolicy, context.CommerceContext) as SellableItem;
            //var saveResult = await _commerceCommander.Pipeline<IPersistEntityPipeline>().Run(new PersistEntityArgument(sellableItem), context);
            return new ImportSellableItemResponse
            {
                SellableItem = arg.SellableItem
            };
        }
        #endregion

        #region Private methods
        /// <summary>
        /// Associate SellableItem with parent Catalog and Category(if exists)
        /// </summary>
        /// <param name="catalogName"></param>
        /// <param name="parentCategoryName"></param>
        /// <param name="sellableItem"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        private async Task AssociateSellableItemWithParentEntities(string catalogName, string parentCategoryName, SellableItem sellableItem, CommerceContext context)
        {
            string parentCategoryCommerceId = null;
            if (!string.IsNullOrEmpty(parentCategoryName))
            {
                var categoryCommerceId = $"{CommerceEntity.IdPrefix<Category>()}{catalogName}-{parentCategoryName}";
                var parentCategory = await _commerceCommander.Command<FindEntityCommand>().Process(context, typeof(Category), categoryCommerceId) as Category;
                parentCategoryCommerceId = parentCategory?.Id;
            }

            var catalogCommerceId = $"{CommerceEntity.IdPrefix<Catalog>()}{catalogName}";
            var sellableItemAssociation = await _commerceCommander.Command<AssociateSellableItemToParentCommand>().Process(context,
                catalogCommerceId,
                parentCategoryCommerceId ?? catalogCommerceId,
                sellableItem.Id);
        }

        /// <summary>
        /// Find and return an existing SellableItem or create a new one
        /// </summary>
        /// <param name="arg"></param>
        /// <param name="context"></param>
        /// <param name="mappingPolicy"></param>
        /// <param name="itemId"></param>
        /// <returns></returns>
        private async Task<SellableItem> GetOrCreateSellableItem(CatalogEntityData entityData, CommercePipelineExecutionContext context)
        {
            var commerceEntityId = $"{CommerceEntity.IdPrefix<SellableItem>()}{entityData.Id}";
            var sellableItem = await _commerceCommander.Command<FindEntityCommand>().Process(context.CommerceContext, typeof(SellableItem), commerceEntityId) as SellableItem;
            if (sellableItem == null)
            {
                await _commerceCommander.Command<CreateSellableItemCommand>().Process(context.CommerceContext, entityData.Id, entityData.Name, entityData.DisplayName, entityData.Description);
                sellableItem = await _commerceCommander.Command<FindEntityCommand>().Process(context.CommerceContext, typeof(SellableItem), commerceEntityId) as SellableItem;
            }

            return sellableItem;
        } 
        #endregion
    }
}