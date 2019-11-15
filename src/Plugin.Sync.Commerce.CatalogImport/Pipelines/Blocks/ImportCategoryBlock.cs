using Newtonsoft.Json.Linq;
using Plugin.Sync.Commerce.CatalogImport.Entities;
using Plugin.Sync.Commerce.CatalogImport.Extensions;
using Plugin.Sync.Commerce.CatalogImport.Pipelines.Arguments;
using Plugin.Sync.Commerce.CatalogImport.Policies;
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
    /// Import data into an existing SellableItem or new SellableItem entity
    /// </summary>
    [PipelineDisplayName("ImportCategoryBlock")]
    public class ImportCategoryBlock : PipelineBlock<ImportSellableItemArgument, ImportCatalogEntityResponse, CommercePipelineExecutionContext>
    {
        #region Private fields
        private readonly CommerceCommander _commerceCommander;
        private readonly ComposerCommander _composerCommander;
        private readonly CommerceEntityImportHelper _importHelper;
        #endregion

        #region Public methods
        /// <summary>
        /// Public contructor
        /// </summary>
        /// <param name="commerceCommander"></param>
        /// <param name="composerCommander"></param>
        /// <param name="importHelper"></param>
        public ImportCategoryBlock(CommerceCommander commerceCommander, ComposerCommander composerCommander)
        {
            _commerceCommander = commerceCommander;
            _composerCommander = composerCommander;
            _importHelper = new CommerceEntityImportHelper(commerceCommander, composerCommander);
        }

        /// <summary>
        /// Main execution point
        /// </summary>
        /// <param name="arg"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public override async Task<ImportCatalogEntityResponse> Run(ImportSellableItemArgument arg, CommercePipelineExecutionContext context)
        {
            await Task.CompletedTask;
            throw new NotImplementedException();
            //TODO: add an option to only import data if Category already exists (don't create a new one)
            //TODO: add an option to only import data if Category don't exist (don't update existing ones)
            //var mappingPolicy = context.CommerceContext.GetPolicy<CategoryMappingPolicy>();
            //_importHelper.AssertRootFields(arg, mappingPolicy);

            //var itemId = arg.JsonData.SelectValue<string>(mappingPolicy.IdPath);
            //Condition.Requires(itemId, "Item ID is reguired in input JSON data").IsNotNullOrEmpty();

            //var catalogName = _importHelper.GetCatalogName(arg.JsonData, mappingPolicy);
            //_importHelper.AssertCatalogExists(context, catalogName);
            //var categoryName = _importHelper.GetParentCategoryName(arg.JsonData, mappingPolicy);

            ////Get or create sellable item
            //Category category = await GetOrCreateCategory(catalogName, categoryName, arg.JsonData, mappingPolicy, context.CommerceContext);

            ////Associate catalog and category
            //await AssociateCategoryWithParentEntities(catalogName, categoryName, category, context.CommerceContext);

            ////Import Category field values
            //category = await _importHelper.ImportComposerViewsFields(category, arg.JsonData, mappingPolicy, context.CommerceContext) as Category;
            //return new ImportSellableItemResponse
            //{
            //    SellableItem = category
            //};
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
        /// Find an existing or create new category
        /// </summary>
        /// <param name="catalogName"></param>
        /// <param name="categoryName"></param>
        /// <param name="jsonData"></param>
        /// <param name="mappingPolicy"></param>
        /// <param name="context"></param>
        /// <returns>Category that has been found or created</returns>
        private async Task<Category> GetOrCreateCategory(string catalogName, string categoryName, JObject jsonData, CategoryMappingPolicy mappingPolicy, CommerceContext context)
        {
            var catalogEntityId = $"{CommerceEntity.IdPrefix<Catalog>()}{catalogName}";
            var categoryEntityId = $"{CommerceEntity.IdPrefix<Category>()}{catalogName}-{categoryName}";

            var category = await _commerceCommander.Command<FindEntityCommand>().Process(context, typeof(Category), categoryEntityId) as Category;
            if (category == null)
            {
                var name = jsonData.SelectValue<string>(mappingPolicy.NamePath) ?? categoryName;
                var displayName = jsonData.SelectValue<string>(mappingPolicy.DisplayNamePath) ?? name;
                var description = jsonData.SelectValue<string>(mappingPolicy.DescriptionPath);

                await _commerceCommander.Command<CreateCategoryCommand>().Process(context, categoryName, name, displayName, description);
                category = await _commerceCommander.Command<FindEntityCommand>().Process(context, typeof(Category), categoryEntityId) as Category;
            }

            return category;
        }
        #endregion
    }
}