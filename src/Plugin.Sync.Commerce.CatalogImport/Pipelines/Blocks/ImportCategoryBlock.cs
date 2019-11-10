using Plugin.Sync.Commerce.CatalogImport.Entities;
using Plugin.Sync.Commerce.CatalogImport.Extensions;
using Plugin.Sync.Commerce.CatalogImport.Helpers;
using Plugin.Sync.Commerce.CatalogImport.Pipelines.Arguments;
using Serilog;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.Core.Commands;
using Sitecore.Commerce.EntityViews;
using Sitecore.Commerce.EntityViews.Commands;
using Sitecore.Commerce.Plugin.Catalog;
using Sitecore.Commerce.Plugin.Pricing;
using Sitecore.Commerce.Plugin.Promotions;
using Sitecore.Framework.Pipelines;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CommerceCatalog = Sitecore.Commerce.Plugin.Catalog;

namespace Plugin.Sync.Commerce.CatalogImport.Pipelines.Blocks
{
    [PipelineDisplayName("ImportCategoryBlock")]
    public class ImportCategoryBlock : PipelineBlock<ImportCategoryArgument, ImportCategoryResponse, CommercePipelineExecutionContext>
    {
        private readonly CommerceCommander _commerceCommander;
        private readonly FindEntityCommand _findEntityCommand;
        private readonly CreateCategoryCommand _createCategoryCommand;
        private readonly CreateCatalogCommand _createCatalogCommand;
        private readonly GetEntityViewCommand _getEntityViewCommand;
        private readonly DoActionCommand _doActionCommand;
        private readonly AssociateCategoryToParentCommand _associateCategoryToParentCommand;
        private readonly EditCategoryCommand _editCategoryCommand;
        
        public ImportCategoryBlock(CommerceCommander commerceCommander, FindEntityCommand findEntityCommand, CreateCategoryCommand createCategoryCommand, CreateCatalogCommand createCatalogCommand,
            GetEntityViewCommand getEntityViewCommand, DoActionCommand doActionCommand, AssociateCategoryToParentCommand associateCategoryToParentCommand, EditCategoryCommand editCategoryCommand)
        {
            _commerceCommander = commerceCommander;
            _findEntityCommand = findEntityCommand;
            _createCategoryCommand = createCategoryCommand;
            _createCatalogCommand = createCatalogCommand;
            _getEntityViewCommand = getEntityViewCommand;
            _doActionCommand = doActionCommand;
            _associateCategoryToParentCommand = associateCategoryToParentCommand;
            _editCategoryCommand = editCategoryCommand;
        }

        public override async Task<ImportCategoryResponse> Run(ImportCategoryArgument arg, CommercePipelineExecutionContext context)
        {
            var request = SyncCategoryData(arg.Features, context.CommerceContext);
            return await request;
        }

        public async Task<ImportCategoryResponse> SyncCategoryData(Dictionary<string, string> data, CommerceContext commerceContext)
        {
            var catalog = GetOrCreateCatalog(data["CatalogId"], commerceContext);
            var commerceCategoryId = CommerceCatalogHelper.GetCommerceCategoryId(data["CatalogId"], data["CategoryId"]);
            var response = await GetOrCreateCategory(commerceCategoryId, catalog.Id, data, commerceContext);
            return response; 
        }

        private CommerceCatalog.Catalog GetOrCreateCatalog(string catalogName, CommerceContext commerceContext)
        {
            var commerceCatalogId = CommerceCatalogHelper.GetCommerceCatalogId(catalogName);
            var catalog = Task.Run<CommerceEntity>(async () => await _findEntityCommand.Process(commerceContext, typeof(Catalog), commerceCatalogId)).Result as Catalog;
            if (catalog == null)
            {
                var catalogBaseName = catalogName.Replace("_Catalog", String.Empty);
                catalog = Task.Run<CommerceCatalog.Catalog>(async () => await _createCatalogCommand.Process(commerceContext, catalogName, catalogName)).Result as Catalog;

                var pricebookname = catalogBaseName + "PriceBook";
                var pricebookId = $"{(object)CommerceEntity.IdPrefix<PriceBook>()}{(object)pricebookname}";
                var pricebook = Task.Run<CommerceEntity>(async () => await _findEntityCommand.Process(commerceContext, typeof(PriceBook), pricebookId)).Result as PriceBook;

                if (pricebook == null)
                {
                    var addPricebookCommand = _commerceCommander.Command<AddPriceBookCommand>();
                    pricebook = Task.Run<PriceBook>(async () => await addPricebookCommand.Process(commerceContext, catalogBaseName + "PriceBook", catalogBaseName + "PriceBook", catalogBaseName + " Book")).Result as PriceBook;
                }

                var promobookname = catalogBaseName + "PromotionsBook";
                var promobookId = $"{(object)CommerceEntity.IdPrefix<PromotionBook>()}{(object)promobookname}";
                var promobook = Task.Run<CommerceEntity>(async () => await _commerceCommander.Command<FindEntityCommand>().Process(commerceContext, typeof(PromotionBook), promobookId)).Result as PromotionBook;

                if (promobook == null)
                {
                    var addPromobookCommand = _commerceCommander.Command<AddPromotionBookCommand>();
                    promobook = Task.Run<PromotionBook>(async () => await addPromobookCommand.Process(commerceContext, promobookname, promobookname, catalogBaseName + " Promotion Book", "")).Result as PromotionBook;
                }

                if (pricebook != null && !String.IsNullOrEmpty(pricebook.Name))
                {
                    catalog.PriceBookName = pricebook.Name;
                }

                if (promobook != null && !String.IsNullOrEmpty(promobook.Name))
                {
                    catalog.PromotionBookName = promobook.Name;
                }

                var result = Task.Run<PersistEntityArgument>(async () => await _commerceCommander.Pipeline<IPersistEntityPipeline>().Run(new PersistEntityArgument(catalog), commerceContext.PipelineContextOptions)).Result;
                catalog = result.Entity as CommerceCatalog.Catalog;
            }

            return catalog;
        }

        private async Task<ImportCategoryResponse> GetOrCreateCategory(string commerceCategoryId, string commerceCatalogId, Dictionary<string, string> data, CommerceContext commerceContext)
        {
            string displayName = data.ContainsKey("DisplayName") && !string.IsNullOrEmpty(data["DisplayName"]) ? data["DisplayName"] : $"{data["CategoryId"]} Category Display Name";
            string description = data.ContainsKey("Description") && !string.IsNullOrEmpty(data["Description"]) ? data["Description"] : $"{data["CategoryId"]} Category Description";
            string errorMessage = string.Empty;

            var category = await _findEntityCommand.Process(commerceContext, typeof(Category), commerceCategoryId) as Category;
            var response = new ImportCategoryResponse();
            if (category == null)
            {
                category = await _createCategoryCommand.Process(commerceContext, commerceCatalogId, data["CategoryId"], displayName, description);
                if (category == null)
                {
                    errorMessage = "Error creating category entity";
                }

                // Associate category with catalog
                var result = await _associateCategoryToParentCommand.Process(commerceContext, commerceCatalogId, commerceCatalogId, category.Id);
                if (result == null)
                {
                    errorMessage = "Error assoicating category with catalog";
                }

                response.StatusCode = 201;
                response.IsNew = true;
            }
            else
            {
                //update category detail
                var catalogContentArgument = await _editCategoryCommand.Process(commerceContext, category, displayName, description);

                if (catalogContentArgument == null)
                {
                    errorMessage = "Error - category details are not updated";
                }

                response.StatusCode = 200;
                response.IsNew = false;
            }

            response.Category = category;

            if (errorMessage.Length >0 )
            {
                throw new ArgumentException(errorMessage);
            }

            return response;
        }

        //private async Task SyncChildViews(Dictionary<string, string> inputData, CommerceContext commerceContext, string commerceSellableItemId)
        //{
        //    var masterView = await _getEntityViewCommand.Process(commerceContext, commerceSellableItemId,
        //        commerceContext.GetPolicy<KnownCatalogViewsPolicy>().Master, "", "");
        //    if (masterView == null)
        //    {
        //        Log.Error($"Master view not found on Sellable Item entity, Entity ID={commerceSellableItemId}");
        //        throw new ApplicationException($"Master view not found on Sellable Item entity, Entity ID={commerceSellableItemId}");
        //    }

        //    if (masterView.ChildViews == null || masterView.ChildViews.Count == 0)
        //    {
        //        Log.Error($"No composer-generated views found on Sellable Item entity, Entity ID={commerceSellableItemId}");
        //        throw new ApplicationException($"No composer-generated views found on Sellable Item entity, Entity ID={commerceSellableItemId}");
        //    }

        //    foreach (var view in masterView.ChildViews)
        //    {
        //        var viewToUpdate = masterView.ChildViews.FirstOrDefault(e => e.Name.Equals(view.Name, StringComparison.OrdinalIgnoreCase)) as EntityView;
        //        if (viewToUpdate != null && viewToUpdate.Properties.Any(p => inputData.ContainsKey(p.Name)))
        //        {
        //            EntityView composerViewForEdit = await _getEntityViewCommand.Process(commerceContext, commerceSellableItemId, "EditView", "EditView", viewToUpdate.ItemId);
        //            if (composerViewForEdit != null)
        //            {
        //                //foreach (var fieldProperty in composerViewForEdit.Properties)
        //                //{
        //                //    fieldProperty.ParseValueAndSetEntityView(inputData);
        //                //}
        //                //var result = await _doActionCommand.Process(commerceContext, composerViewForEdit);
        //                var isUpdated = false;
        //                foreach (var fieldProperty in composerViewForEdit.Properties)
        //                {
        //                    if (inputData.ContainsKey(fieldProperty.Name))
        //                    {
        //                        fieldProperty.ParseValueAndSetEntityView(inputData[fieldProperty.Name]);
        //                        isUpdated = true;
        //                    }

        //                }

        //                if (isUpdated)
        //                {
        //                    var result = await _doActionCommand.Process(commerceContext, composerViewForEdit);
        //                }
        //            }
        //        }
        //    }
        //}
    }
}