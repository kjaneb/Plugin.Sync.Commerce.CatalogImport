using Plugin.Sync.Commerce.CatalogImport.Extensions;
using Plugin.Sync.Commerce.CatalogImport.Models;
using Plugin.Sync.Commerce.CatalogImport.Pipelines.Arguments;
using Serilog;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.Core.Commands;
using Sitecore.Commerce.EntityViews;
using Sitecore.Commerce.EntityViews.Commands;
using Sitecore.Commerce.Plugin.Catalog;
using Sitecore.Commerce.Plugin.Composer;
using Sitecore.Framework.Conditions;
using Sitecore.Framework.Pipelines;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Plugin.Sync.Commerce.CatalogImport.Pipelines.Blocks
{
    /// <summary>
    /// Import data into an existing SellableItem or new SellableItem entity
    /// </summary>
    [PipelineDisplayName("UpdateComposerFieldsBlock")]
    public class UpdateComposerFieldsBlock : PipelineBlock<ImportCatalogEntityArgument, ImportCatalogEntityArgument, CommercePipelineExecutionContext>
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
        public UpdateComposerFieldsBlock(CommerceCommander commerceCommander, ComposerCommander composerCommander)
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
        public override async Task<ImportCatalogEntityArgument> Run(ImportCatalogEntityArgument arg, CommercePipelineExecutionContext context)
        {
           
                
            CommerceEntity entity = null;
            var entityDataModel = context.GetModel<CatalogEntityDataModel>();
            Condition.Requires(entityDataModel, "CatalogEntityDataModel is required to exist in order for CommercePipelineExecutionContext to run").IsNotNull();
            Condition.Requires(entityDataModel.EntityId, "EntityId is reguired in input JSON data").IsNotNullOrEmpty();

            if (entityDataModel != null)
            {
                var commerceEntityId = $"{CommerceEntity.IdPrefix<SellableItem>()}{entityDataModel.EntityId}";
                entity = await _commerceCommander.Command<FindEntityCommand>().Process(context.CommerceContext, typeof(CommerceEntity), commerceEntityId);
                if (entity == null)
                {
                    var errorMessage = $"Error: Commerce Entity with ID={entityDataModel.EntityId} not found, UpdateComposerFieldsBlock cannot be executed.";
                    Log.Error(errorMessage);
                    context.Abort(errorMessage, this);
                    return arg;
                }

                await ImportComposerViewsFields(entity, entityDataModel.ComposerFields, context.CommerceContext);
            }
            else
            {
                var errorMessage = $"Error: SellableItemEntityData or CategoryEntityData model is required to be present in CommercePipelineExecutionContext for UpdateComposerFieldsBlock to run.";
                Log.Error(errorMessage);
                context.Abort(errorMessage, this);
                return arg;
            }
            
            
            return arg;
        }
        #endregion

        
        public async Task<bool> ImportComposerViewsFields(CommerceEntity commerceEntity, Dictionary<string, string> composerFields, CommerceContext context)
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
            foreach (EntityView view in masterView.ChildViews)
            {
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
            }

            if (isUpdated)
            {
                return await _composerCommander.PersistEntity(context, commerceEntity);
            }

            return false;
        }

    }
}