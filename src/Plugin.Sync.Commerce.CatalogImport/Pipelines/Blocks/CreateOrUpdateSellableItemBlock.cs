using Plugin.Sync.Commerce.CatalogImport.Extensions;
using Plugin.Sync.Commerce.CatalogImport.Models;
using Plugin.Sync.Commerce.CatalogImport.Pipelines.Arguments;
using Serilog;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.Core.Commands;
using Sitecore.Commerce.Plugin.Catalog;
using Sitecore.Commerce.Plugin.Composer;
using Sitecore.Framework.Conditions;
using Sitecore.Framework.Pipelines;
using System;
using System.Threading.Tasks;

namespace Plugin.Sync.Commerce.CatalogImport.Pipelines.Blocks
{
    /// <summary>
    /// Import data into an existing or a new SellableItem entity
    /// </summary>
    [PipelineDisplayName("CreateOrUpdateSellableItemBlock")]
    public class CreateOrUpdateSellableItemBlock : PipelineBlock<ImportCatalogEntityArgument, ImportCatalogEntityArgument, CommercePipelineExecutionContext>
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
        public CreateOrUpdateSellableItemBlock(CommerceCommander commerceCommander, ComposerCommander composerCommander)
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
        public override async Task<ImportCatalogEntityArgument> Run(ImportCatalogEntityArgument arg, CommercePipelineExecutionContext context)
        {
            //TODO: add an option to only import data if SellableItem already exists (don't create a new one)
            //TODO: add an option to only import data if SellableItem don't exist (don't update existing ones)

            var entityData = context.GetModel<CatalogEntityDataModel>();

            Condition.Requires(entityData, "CatalogEntityDataModel is required to exist in order for CommercePipelineExecutionContext to run").IsNotNull();
            Condition.Requires(entityData.EntityId, "EntityId is reguired in input JSON data").IsNotNullOrEmpty();
            Condition.Requires(entityData.EntityName, "EntityName is reguired in input JSON data").IsNotNullOrEmpty();
            Condition.Requires(entityData.ParentCatalogName, "ParentCatalogName Name is reguired to be present in input JSON data or set default in SellabeItemMappingPolicy").IsNotNullOrEmpty();

            try
            {
                //Get or create sellable item
                var sellableItem = await GetOrCreateSellableItem(entityData, context);
                //Associate catalog and category
                sellableItem = await AssociateSellableItemWithParentEntities(entityData.ParentCatalogName, entityData.ParentCategoryName, sellableItem, context.CommerceContext);

                //Check code running before this - this persist might be redindant
                //var persistResult = await _commerceCommander.Pipeline<IPersistEntityPipeline>().Run(new PersistEntityArgument(sellableItem), context);
                if (sellableItem == null)
                {
                    var errorMessage = $"Error persisting changes to SellableItem Entity withID == {entityData.EntityId}.";
                    Log.Error(errorMessage);
                    context.Abort(errorMessage, this);
                    //TODO: cleanup response
                    return arg;
                }

                return arg;
            }
            catch (Exception ex)
            {
                var errorMessage = $"Error creating or updating SellableItem Entity {entityData.EntityId}. {ex.Message}";
                Log.Error(ex, errorMessage);
                context.Abort(errorMessage, ex);
                return arg;
            }
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
        private async Task<SellableItem> AssociateSellableItemWithParentEntities(string catalogName, string parentCategoryName, SellableItem sellableItem, CommerceContext context)
        {
            string parentCategoryCommerceId = null;
            if (!string.IsNullOrEmpty(parentCategoryName))
            {
                var categoryCommerceId = $"{CommerceEntity.IdPrefix<Category>()}{catalogName}-{parentCategoryName}";
                var parentCategory = await _commerceCommander.Command<FindEntityCommand>().Process(context, typeof(Category), categoryCommerceId) as Category;
                parentCategoryCommerceId = parentCategory?.Id;
            }

            //TODO: Delete old relationships
            //var deassociateResult = await _commerceCommander.Command<DeleteRelationshipCommand>().Process(context, oldParentCategory.Id, sellableItem.Id, "CategoryToSellableItem");

            var catalogCommerceId = $"{CommerceEntity.IdPrefix<Catalog>()}{catalogName}";
            var sellableItemAssociation = await _commerceCommander.Command<AssociateSellableItemToParentCommand>().Process(context,
                catalogCommerceId,
                parentCategoryCommerceId ?? catalogCommerceId,
                sellableItem.Id);

            return await _commerceCommander.Command<FindEntityCommand>().Process(context, typeof(SellableItem), sellableItem.Id) as SellableItem;
        }

        /// <summary>
        /// Find and return an existing Category or create a new one
        /// </summary>
        /// <param name="entityData"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        private async Task<SellableItem> GetOrCreateSellableItem(CatalogEntityDataModel entityData, CommercePipelineExecutionContext context)
        {
            SellableItem sellableItem = await _commerceCommander.Command<FindEntityCommand>().Process(context.CommerceContext, typeof(SellableItem), entityData.CommerceEntityId) as SellableItem;
            if (sellableItem == null)
            {
                sellableItem = await _commerceCommander.Command<CreateSellableItemCommand>().Process(context.CommerceContext,
                    entityData.EntityId,
                    entityData.EntityName,
                    entityData.EntityFields.ContainsKey("DisplayName") ? entityData.EntityFields["DisplayName"] : entityData.EntityName,
                    entityData.EntityFields.ContainsKey("Description") ? entityData.EntityFields["Description"] : string.Empty,
                    entityData.EntityFields.ContainsKey("Brand") ? entityData.EntityFields["Brand"] : string.Empty,
                    entityData.EntityFields.ContainsKey("Manufacturer") ? entityData.EntityFields["Manufacturer"] : string.Empty,
                    entityData.EntityFields.ContainsKey("TypeOfGoods") ? entityData.EntityFields["TypeOfGoods"] : string.Empty);
            }
            else
            {
                sellableItem.DisplayName = entityData.EntityFields.ContainsKey("DisplayName") ? entityData.EntityFields["DisplayName"] : entityData.EntityName;
                sellableItem.Description = entityData.EntityFields.ContainsKey("Description") ? entityData.EntityFields["Description"] : string.Empty;
                sellableItem.Brand = entityData.EntityFields.ContainsKey("Brand") ? entityData.EntityFields["Brand"] : string.Empty;
                sellableItem.Manufacturer = entityData.EntityFields.ContainsKey("Manufacturer") ? entityData.EntityFields["Manufacturer"] : string.Empty;
                sellableItem.TypeOfGood = entityData.EntityFields.ContainsKey("TypeOfGoods") ? entityData.EntityFields["TypeOfGoods"] : string.Empty;

                var persistResult = await _commerceCommander.Pipeline<IPersistEntityPipeline>().Run(new PersistEntityArgument(sellableItem), context);
            }

            return await _commerceCommander.Command<FindEntityCommand>().Process(context.CommerceContext, typeof(SellableItem), sellableItem.Id) as SellableItem;
        }
        #endregion
    }
}