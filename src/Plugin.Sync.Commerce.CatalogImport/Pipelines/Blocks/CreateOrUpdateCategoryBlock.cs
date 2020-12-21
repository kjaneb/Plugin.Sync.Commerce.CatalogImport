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
    /// Import data into an existing Category or new SellableItem entity
    /// </summary>
    [PipelineDisplayName("CreateOrUpdateCategoryBlock")]
    public class CreateOrUpdateCategoryBlock : AsyncPipelineBlock<ImportCatalogEntityArgument, ImportCatalogEntityArgument, CommercePipelineExecutionContext>
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
        public CreateOrUpdateCategoryBlock(CommerceCommander commerceCommander, ComposerCommander composerCommander)
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
        public override async Task<ImportCatalogEntityArgument> RunAsync(ImportCatalogEntityArgument arg, CommercePipelineExecutionContext context)
        {
            //TODO: add an option to only import data if SellableItem already exists (don't create a new one)
            //TODO: add an option to only import data if SellableItem don't exist (don't update existing ones)

            var entityData = context.GetModel<CatalogEntityDataModel>();

            Condition.Requires(entityData, "CatalogEntityDataModel is required to exist in order for CommercePipelineExecutionContext to run").IsNotNull();
            Condition.Requires(entityData.EntityId, "EntityId is reguired in input JSON data").IsNotNullOrEmpty();
            Condition.Requires(entityData.EntityName, "EntityName is reguired in input JSON data").IsNotNullOrEmpty();
            Condition.Requires(entityData.CommerceEntityId, "Commerce Entity ID cannot be identified based on input JSON data").IsNotNullOrEmpty();
            Condition.Requires(entityData.ParentCatalogName, "ParentCatalogName Name is reguired to be present in input JSON data or set default in SellabeItemMappingPolicy").IsNotNullOrEmpty();

            try
            {
                //Get or create sellable item
                var category = await GetOrCreateCategory(entityData, context);
                //Associate catalog and category
                category = await AssociateCategoryWithParentEntities(entityData.ParentCatalogName, entityData.ParentCategoryName, category, context.CommerceContext);

                //Check code running before this - this persist might be redindant
                //var persistResult = await _commerceCommander.Pipeline<IPersistEntityPipeline>().Run(new PersistEntityArgument(category), context);
                if (category == null)
                {
                    var errorMessage = $"Error persisting changes to Category Entity withID == {entityData.EntityId}.";
                    Log.Error(errorMessage);
                    context.Abort(errorMessage, this);
                    //TODO: cleanup response
                    return arg;
                }

                return arg;
            }
            catch (Exception ex)
            {
                var errorMessage = $"Error creating or updating Category Entity {entityData.EntityId}. {ex.Message}";
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
        /// <param name="category"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        private async Task<Category> AssociateCategoryWithParentEntities(string catalogName, string parentCategoryName, Category category, CommerceContext context)
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
            var sellableItemAssociation = await _commerceCommander.Command<AssociateCategoryToParentCommand>().Process(context,
                catalogCommerceId,
                parentCategoryCommerceId ?? catalogCommerceId,
                category.Id);

            return await _commerceCommander.Command<FindEntityCommand>().Process(context, typeof(Category), category.Id) as Category;
        }

        /// <summary>
        /// Find and return an existing Category or create a new one
        /// </summary>
        /// <param name="entityData"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        private async Task<Category> GetOrCreateCategory(CatalogEntityDataModel entityData, CommercePipelineExecutionContext context)
        {
            Category category = await _commerceCommander.Command<FindEntityCommand>().Process(context.CommerceContext, typeof(Category), entityData.CommerceEntityId) as Category;
            if (category == null)
            {
                category = await _commerceCommander.Command<CreateCategoryCommand>().Process(context.CommerceContext,
                    entityData.ParentCatalogName,
                    entityData.EntityName,
                    entityData.EntityFields.ContainsKey("DisplayName") ? entityData.EntityFields["DisplayName"] : entityData.EntityName,
                    entityData.EntityFields.ContainsKey("Description") ? entityData.EntityFields["Description"] : string.Empty);
            }
            else
            {
                category.DisplayName = entityData.EntityFields.ContainsKey("DisplayName") ? entityData.EntityFields["DisplayName"] : entityData.EntityName;
                category.Description = entityData.EntityFields.ContainsKey("Description") ? entityData.EntityFields["Description"] : string.Empty;

                var persistResult = await _commerceCommander.Pipeline<IPersistEntityPipeline>().RunAsync(new PersistEntityArgument(category), context);
            }

            return await _commerceCommander.Command<FindEntityCommand>().Process(context.CommerceContext, typeof(Category), category.Id) as Category;
        }
        #endregion
    }
}