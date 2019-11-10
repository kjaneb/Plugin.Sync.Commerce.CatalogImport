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
using System.Threading.Tasks;

namespace Plugin.Sync.Commerce.CatalogImport.Pipelines.Blocks
{
    /// <summary>
    /// Import data into an existing SellableItem or new SellableItem entity
    /// </summary>
    [PipelineDisplayName("ImportSellableItemBlock")]
    public class ImportSellableItemBlock : PipelineBlock<ImportCommerceEntityArgument, ImportCommerceEntityResponse, CommercePipelineExecutionContext>
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
        public ImportSellableItemBlock(CommerceCommander commerceCommander, ComposerCommander composerCommander, CommerceEntityImportHelper importHelper
            )
        {
            _commerceCommander = commerceCommander;
            _composerCommander = composerCommander;
            _importHelper = importHelper;
        }

        /// <summary>
        /// Main execution point
        /// </summary>
        /// <param name="arg"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public override async Task<ImportCommerceEntityResponse> Run(ImportCommerceEntityArgument arg, CommercePipelineExecutionContext context)
        {
            //TODO: add an option to only import data if SellableItem already exists (don't create a new one)
            //TODO: add an option to only import data if SellableItem don't exist (don't update existing ones)
            var mappingPolicy = context.CommerceContext.GetPolicy<SellableItemMappingPolicy>();
            _importHelper.AssertRootFields(arg, mappingPolicy);

            var itemId = arg.JsonData.SelectValue<string>(mappingPolicy.IdPath);
            Condition.Requires(itemId, "Item ID is reguired in input JSON data").IsNotNullOrEmpty();

            var catalogName = _importHelper.GetCatalogName(arg.JsonData, mappingPolicy);
            _importHelper.AssertCatalogExists(context, catalogName);

            //Get or create sellable item
            SellableItem sellableItem = await GetOrCreateSellableItem(arg.JsonData, context, mappingPolicy, itemId);

            //Associate catalog and category
            var categoryName = _importHelper.GetParentCategoryName(arg.JsonData, mappingPolicy);
            await AssociateSellableItemWithParentEntities(catalogName, categoryName, sellableItem, context.CommerceContext);

            //Import SellableIltem field values
            sellableItem = await _importHelper.ImportComposerViewsFields(sellableItem, arg.JsonData, mappingPolicy, context.CommerceContext) as SellableItem;
            return new ImportCommerceEntityResponse
            {
                CommerceEntity = sellableItem
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
        private async Task<SellableItem> GetOrCreateSellableItem(JObject jsonData, CommercePipelineExecutionContext context, SellableItemMappingPolicy mappingPolicy, string itemId)
        {
            var commerceEntityId = $"{CommerceEntity.IdPrefix<SellableItem>()}{itemId}";
            var sellableItem = await _commerceCommander.Command<FindEntityCommand>().Process(context.CommerceContext, typeof(SellableItem), commerceEntityId) as SellableItem;
            if (sellableItem == null)
            {
                var name = jsonData.SelectValue<string>(mappingPolicy.NamePath) ?? itemId;
                var displayName = jsonData.SelectValue<string>(mappingPolicy.DisplayNamePath) ?? name;
                var description = jsonData.SelectValue<string>(mappingPolicy.DescriptionPath);

                await _commerceCommander.Command<CreateSellableItemCommand>().Process(context.CommerceContext, itemId, name, displayName, description);
                sellableItem = await _commerceCommander.Command<FindEntityCommand>().Process(context.CommerceContext, typeof(SellableItem), commerceEntityId) as SellableItem;
            }

            return sellableItem;
        } 
        #endregion
    }
}