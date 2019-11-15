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
    [PipelineDisplayName("ImportCategoryCreateOrUpdateEntityBlock")]
    public class ImportCategoryCreateOrUpdateEntityBlock : PipelineBlock<ImportCategoryArgument, ImportCategoryArgument, CommercePipelineExecutionContext>
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
        public ImportCategoryCreateOrUpdateEntityBlock(CommerceCommander commerceCommander, ComposerCommander composerCommander)
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
        public override async Task<ImportCategoryArgument> Run(ImportCategoryArgument arg, CommercePipelineExecutionContext context)
        {
            //TODO: add an option to only import data if SellableItem already exists (don't create a new one)
            //TODO: add an option to only import data if SellableItem don't exist (don't update existing ones)
            //_importHelper.AssertRootFields(arg, mappingPolicy);

            //var itemId = arg.JsonData.SelectValue<string>(mappingPolicy.IdPath);
            //Condition.Requires(itemId, "Item ID is reguired in input JSON data").IsNotNullOrEmpty();

            Condition.Requires(arg.EntityData.Id, "Item ID is reguired in input JSON data").IsNotNullOrEmpty();
            Condition.Requires(arg.EntityData.CatalogName, "Catalog Name is reguired to be present in input JSON data or set default in SellabeItemMappingPolicy").IsNotNullOrEmpty();

            try
            {
                //Get or create sellable item
                arg.Category = await GetOrCreateCategory(arg.EntityData, context);
                //Associate catalog and category
                await AssociateCategoryWithParentEntities(arg.EntityData.CatalogName, arg.EntityData.ParentCategoryName, arg.Category, context.CommerceContext);

                //Check code running before this - this persist might be redindant
                var persistResult = await _commerceCommander.Pipeline<IPersistEntityPipeline>().Run(new PersistEntityArgument(arg.Category), context);
                if (persistResult == null || !persistResult.Success)
                {
                    var errorMessage = $"Error persisting changes to Sellable Item {arg.EntityData.Id}.";
                    Log.Error(errorMessage);
                    context.Abort(errorMessage, this);
                    //TODO: cleanup response
                    return arg;
                }
                
            }
            catch (Exception ex)
            {
                var errorMessage = $"Error creating or updating Sellable Item {arg.EntityData.Id}. {ex.Message}";
                Log.Error(ex, errorMessage);
                context.Abort(errorMessage, ex);
                return arg;
            }
            //TODO: see if persistResult.Entity is really up to date and return it instead of below line result if so
            arg.Category = await _commerceCommander.Command<FindEntityCommand>().Process(context.CommerceContext, typeof(Category), arg.Category.Id) as Category;
            return arg;

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
        private async Task AssociateCategoryWithParentEntities(string catalogName, string parentCategoryName, Category category, CommerceContext context)
        {
            string parentCategoryCommerceId = null;
            if (!string.IsNullOrEmpty(parentCategoryName))
            {
                var categoryCommerceId = $"{CommerceEntity.IdPrefix<Category>()}{catalogName}-{parentCategoryName}";
                var parentCategory = await _commerceCommander.Command<FindEntityCommand>().Process(context, typeof(Category), categoryCommerceId) as Category;
                parentCategoryCommerceId = parentCategory?.Id;
            }

            var catalogCommerceId = $"{CommerceEntity.IdPrefix<Catalog>()}{catalogName}";
            var sellableItemAssociation = await _commerceCommander.Command<AssociateCategoryToParentCommand>().Process(context,
                catalogCommerceId,
                parentCategoryCommerceId ?? catalogCommerceId,
                category.Id);
        }

        /// <summary>
        /// Find and return an existing Category or create a new one
        /// </summary>
        /// <param name="entityData"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        private async Task<Category> GetOrCreateCategory(CategoryEntityData entityData, CommercePipelineExecutionContext context)
        {
            var commerceEntityId = $"{CommerceEntity.IdPrefix<Category>()}{entityData.Id}";
            var category = await _commerceCommander.Command<FindEntityCommand>().Process(context.CommerceContext, typeof(Category), commerceEntityId) as Category;
            if (category == null)
            {
                await _commerceCommander.Command<CreateCategoryCommand>().Process(context.CommerceContext,
                    entityData.Id,
                    entityData.Name,
                    entityData.DisplayName,
                    entityData.Description);
            }
            else
            {
                category.Description = entityData.Description;
                category.DisplayName = entityData.DisplayName;

                var persistResult = await _commerceCommander.Pipeline<IPersistEntityPipeline>().Run(new PersistEntityArgument(category), context);
            }

            return await _commerceCommander.Command<FindEntityCommand>().Process(context.CommerceContext, typeof(Category), commerceEntityId) as Category;
        } 
        #endregion
    }
}