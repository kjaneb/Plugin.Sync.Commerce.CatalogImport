using Newtonsoft.Json.Linq;
using Plugin.Sync.Commerce.CatalogImport.Policies;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.EntityViews;
using Sitecore.Commerce.Plugin.Catalog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Serilog;
using Sitecore.Commerce.EntityViews.Commands;
using Sitecore.Commerce.Plugin.Composer;
using Sitecore.Commerce.Core.Commands;

namespace Plugin.Sync.Commerce.CatalogImport.Extensions
{
    public class CommerceEntityImportHelper
    {
        private readonly CommerceCommander _commerceCommander;
        public void AssetCatalogExists(CommercePipelineExecutionContext context, string catalogName)
        {
            var commerceCatalogId = $"{CommerceEntity.IdPrefix<Catalog>()}{catalogName}";
            Catalog catalog = Task.Run<CommerceEntity>(async () => await _commerceCommander.Command<FindEntityCommand>().Process(context.CommerceContext, typeof(Catalog), commerceCatalogId)).Result as Catalog;
            if (catalog == null)
            {
                throw new ArgumentException($"Catalog '{catalogName}' not found");
            }
        }

        public async Task AssociateWithParentCategory(CommercePipelineExecutionContext context, string catalogName, CommerceEntity commerceEntity, string categoryName)
        {
            string parentCategoryCommerceId = null;
            if (!string.IsNullOrEmpty(categoryName))
            {
                var categoryCommerceId = $"{CommerceEntity.IdPrefix<Category>()}{catalogName}-{categoryName}";
                var parentCategory = await _commerceCommander.Command<FindEntityCommand>().Process(context.CommerceContext, typeof(Category), categoryCommerceId) as Category;
                parentCategoryCommerceId = parentCategory?.Id;
            }

            var catalogCommerceId = $"{CommerceEntity.IdPrefix<Catalog>()}{catalogName}";
            var sellableItemAssociation = await _commerceCommander.Command<AssociateSellableItemToParentCommand>().Process(context.CommerceContext,
                catalogCommerceId,
                parentCategoryCommerceId ?? catalogCommerceId,
                commerceEntity.Id);
        }

        public CommerceEntityImportHelper(CommerceCommander commerceCommander)
        {
            _commerceCommander = commerceCommander;
        }
        public async Task<CommerceEntity> SyncChildViews(CommerceEntity commerceEntity, JObject jsonData, MappingPolicyBase mappingPolicy, CommerceContext context)
        {
            var masterView = await _commerceCommander.Command<GetEntityViewCommand>().Process(
                context, commerceEntity.Id,
                commerceEntity.EntityVersion,
                context.GetPolicy<KnownCatalogViewsPolicy>().Master,
                string.Empty,
                string.Empty);

            if (masterView == null)
            {
                Log.Error($"Master view not found on Commerce Entity, Entity ID={commerceEntity.Id}");
                throw new ApplicationException($"Master view not found on Commerce Entity, Entity ID={commerceEntity.Id}");
            }

            if (masterView.ChildViews == null || masterView.ChildViews.Count == 0)
            {
                Log.Error($"No composer-generated views found on Sellable Item entity, Entity ID={commerceEntity.Id}");
                throw new ApplicationException($"No composer-generated views found on Sellable Item entity, Entity ID={commerceEntity.Id}");
            }

            var isUpdated = false;
            foreach (var view in masterView.ChildViews)
            {
                var viewToUpdate = masterView.ChildViews.FirstOrDefault(e => e.Name.Equals(view.Name, StringComparison.OrdinalIgnoreCase)) as EntityView;
                if (viewToUpdate != null)
                {
                    var composerViewForEdit = Task.Run<EntityView>(async () => await commerceEntity.GetComposerView(viewToUpdate.ItemId, _commerceCommander, context)).Result;
                    if (composerViewForEdit != null)
                    {
                        foreach (var fieldProperty in composerViewForEdit.Properties)
                        {
                            var propertyPath = mappingPolicy.FieldPaths.ContainsKey(fieldProperty.Name) ? mappingPolicy.FieldPaths[fieldProperty.Name] : null;
                            var fieldValue = jsonData.QueryMappedValue<string>(fieldProperty.Name, propertyPath, mappingPolicy.RootPaths);
                            if (!string.IsNullOrEmpty(fieldValue))
                            {
                                fieldProperty.ParseValueAndSetEntityView(fieldValue);
                                isUpdated = true;
                            }
                            else if (mappingPolicy.ClearFieldValues)
                            {
                                fieldProperty.RawValue = string.Empty;
                                fieldProperty.Value = string.Empty;
                                isUpdated = true;
                            }
                        }
                    }
                }
            }

            if (isUpdated)
            {
                await _composerCommander.PersistEntity(context, commerceEntity);
            }

            return await _findEntityCommand.Process(context, typeof(SellableItem), commerceEntity.Id);
        }

    }
}
