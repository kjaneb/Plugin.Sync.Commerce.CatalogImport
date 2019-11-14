using Newtonsoft.Json.Linq;
using Plugin.Sync.Commerce.CatalogImport.Pipelines.Arguments;
using Plugin.Sync.Commerce.CatalogImport.Policies;
using Serilog;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.Core.Commands;
using Sitecore.Commerce.EntityViews;
using Sitecore.Commerce.EntityViews.Commands;
using Sitecore.Commerce.Plugin.Catalog;
using Sitecore.Commerce.Plugin.Composer;
using Sitecore.Framework.Conditions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Plugin.Sync.Commerce.CatalogImport.Extensions
{
    /// <summary>
    /// Helper methods shared by Import Sellable Item and Import Category blocks
    /// </summary>
    public class CommerceEntityImportHelper
    {
        #region Private fields
        private readonly CommerceCommander _commerceCommander;
        private readonly ComposerCommander _composerCommander;
        #endregion

        /// <summary>
        /// Public constructor
        /// </summary>
        /// <param name="commerceCommander"></param>
        /// <param name="composerCommander"></param>
        public CommerceEntityImportHelper(CommerceCommander commerceCommander, ComposerCommander composerCommander)
        {
            _commerceCommander = commerceCommander;
            _composerCommander = composerCommander;
        }

        #region Public methods
        /// <summary>
        /// Check if Catalog entity exists in Commerce DB and thorw exception if not
        /// </summary>
        /// <param name="context"></param>
        /// <param name="catalogName"></param>
        public void AssertCatalogExists(CommercePipelineExecutionContext context, string catalogName)
        {
            var commerceCatalogId = $"{CommerceEntity.IdPrefix<Catalog>()}{catalogName}";
            Catalog catalog = Task.Run<CommerceEntity>(async () => await _commerceCommander.Command<FindEntityCommand>().Process(context.CommerceContext, typeof(Catalog), commerceCatalogId)).Result as Catalog;
            if (catalog == null)
            {
                throw new ArgumentException($"Catalog '{catalogName}' not found");
            }
        }

        /// <summary>
        /// Assert if all main fields are present in input Json Object, throw exception if one or more are missing
        /// </summary>
        /// <param name="arg"></param>
        /// <param name="mappingPolicy"></param>
        public void AssertRootFields(ImportSellableItemArgument arg, MappingPolicyBase mappingPolicy)
        {
            Condition.Requires(mappingPolicy, nameof(mappingPolicy)).IsNotNull();
            Condition.Requires(arg.JsonData, nameof(arg.JsonData)).IsNotNull();
            Condition.Requires(mappingPolicy.IdPath, nameof(mappingPolicy.IdPath)).IsNotNullOrEmpty();
            Condition.Requires(mappingPolicy.NamePath, nameof(mappingPolicy.NamePath)).IsNotNullOrEmpty();
        }

        /// <summary>
        /// Get Catalog name from input Json or fallback to default Catalog name in Mapping Policy configuration
        /// </summary>
        /// <param name="jsonData"></param>
        /// <param name="mappingPolicy"></param>
        /// <returns></returns>
        public string GetCatalogName(JObject jsonData, MappingPolicyBase mappingPolicy)
        {
            var catalogName = jsonData.SelectValue<string>(mappingPolicy.CatalogNamePath);
            if (string.IsNullOrEmpty(catalogName) && !string.IsNullOrEmpty(mappingPolicy.DefaultCatalogName))
            {
                catalogName = mappingPolicy.DefaultCatalogName;
            }
            return catalogName;
        }

        /// <summary>
        /// Get Parent Category name from input Json or fallback to default Category name in Mapping Policy configuration
        /// </summary>
        /// <param name="jsonData"></param>
        /// <param name="mappingPolicy"></param>
        /// <returns></returns>
        public string GetParentCategoryName(JObject jsonData, MappingPolicyBase mappingPolicy)
        {
            var categoryName = jsonData.SelectValue<string>(mappingPolicy.ParentCategoryNamePath);
            if (string.IsNullOrEmpty(categoryName) && !string.IsNullOrEmpty(mappingPolicy.DefaultCategoryName))
            {
                categoryName = mappingPolicy.DefaultCategoryName;
            }

            return categoryName;
        }

        /// <summary>
        /// Import fields defined in Item's composer views
        /// </summary>
        /// <param name="commerceEntity"></param>
        /// <param name="jsonData"></param>
        /// <param name="mappingPolicy"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task<CommerceEntity> ImportComposerViewsFields(CommerceEntity commerceEntity, Dictionary<string, string> composerFields, CommerceContext context)
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

            //masterView.
            //_commerceCommander.Command<EditSellableItemCommand>().Process()
            var isUpdated = false;
            foreach (EntityView view in masterView.ChildViews)
            {
                var viewToEdit = await _commerceCommander.Command<GetEntityViewCommand>().Process(
                        context, 
                        commerceEntity.Id,
                        commerceEntity.EntityVersion,
                        view.Name,
                        string.Empty,
                        string.Empty);
                //}
                EntityView composerViewForEdit = null;
                foreach (var viewField in view.Properties)
                {
                    if (composerFields.Keys.Contains(viewField.Name))
                    {
                        if (composerViewForEdit == null)
                        {
                            composerViewForEdit = Task.Run<EntityView>(async () => await commerceEntity.GetComposerView(view.ItemId, _commerceCommander, context)).Result;
                        }
                        if (composerViewForEdit != null)
                        {
                            var composerProperty = composerViewForEdit.GetProperty(viewField.Name);
                            if (composerViewForEdit != null)
                            {
                                composerProperty.ParseValueAndSetEntityView(composerFields[viewField.Name]);
                                isUpdated = true;
                            }
                        }
                    }
                }

                //masterView.ChildViews.FirstOrDefault(e => e.Name.Equals(view.Name, StringComparison.OrdinalIgnoreCase)) as EntityView;


                //var viewToUpdate = masterView.ChildViews.FirstOrDefault(e => e.Name.Equals(view.Name, StringComparison.OrdinalIgnoreCase)) as EntityView;
                //if (viewToUpdate != null)
                //{
                //    var composerViewForEdit = Task.Run<EntityView>(async () => await commerceEntity.GetComposerView(viewToUpdate.ItemId, _commerceCommander, context)).Result;
                //    if (composerViewForEdit != null)
                //    {
                //        foreach (var fieldProperty in composerViewForEdit.Properties)
                //        {
                //            //var propertyPath = mappingPolicy.FieldPaths.ContainsKey(fieldProperty.Name) ? mappingPolicy.FieldPaths[fieldProperty.Name] : null;
                //            //var fieldValue = jsonData.QueryMappedValue<string>(fieldProperty.Name, propertyPath, mappingPolicy.RootPaths);
                //            //if (!string.IsNullOrEmpty(fieldValue))
                //            //{
                //            //    fieldProperty.ParseValueAndSetEntityView(fieldValue);
                //            //    isUpdated = true;
                //            //}
                //            //else if (mappingPolicy.ClearFieldValues)
                //            //{
                //            //    fieldProperty.RawValue = string.Empty;
                //            //    fieldProperty.Value = string.Empty;
                //            //    isUpdated = true;
                //            //}
                //        }
                //    }
                //}
            }

            if (isUpdated)
            {
                //var result = await _commerceCommander.Pipeline<IPersistEntityPipeline>().Run(new PersistEntityArgument(commerceEntity), context);
                await _composerCommander.PersistEntity(context, commerceEntity);
                //await _commerceCommander.PersistEntity(context, commerceEntity);
                return await _commerceCommander.Command<FindEntityCommand>().Process(context, typeof(CommerceEntity), commerceEntity.Id);

            }

            return commerceEntity;
            //return await _commerceCommander.Command<FindEntityCommand>().Process(context, typeof(CommerceEntity), commerceEntity.Id);
        }

        #endregion
    }
}
