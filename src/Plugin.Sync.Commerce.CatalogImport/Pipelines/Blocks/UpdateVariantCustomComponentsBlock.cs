using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Plugin.Sync.Commerce.CatalogImport.Extensions;
using Plugin.Sync.Commerce.CatalogImport.Pipelines.Arguments;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.Plugin.Composer;
using Sitecore.Framework.Pipelines;
using System.Threading.Tasks;
using Plugin.Sync.Commerce.CatalogImport.Models;
using Serilog;
using Sitecore.Commerce.Core.Commands;
using Sitecore.Commerce.Plugin.Catalog;
using Sitecore.Framework.Conditions;

namespace Plugin.Sync.Commerce.CatalogImport.Pipelines.Blocks
{
    /// <summary>
    /// Import data into an existing SellableItem or new SellableItem entity
    /// </summary>
    [PipelineDisplayName("UpdateCustomComponentsBlock")]
    public class UpdateVariantCustomComponentsBlock : AsyncPipelineBlock<ImportCatalogEntityArgument, ImportCatalogEntityArgument, CommercePipelineExecutionContext>
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
        public UpdateVariantCustomComponentsBlock(CommerceCommander commerceCommander, ComposerCommander composerCommander)
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
        public override async Task<ImportCatalogEntityArgument> RunAsync(ImportCatalogEntityArgument arg, CommercePipelineExecutionContext context)
        {
            CommerceEntity entity = null;
            var entityDataModel = context.GetModel<CatalogEntityDataModel>();
            Condition.Requires(entityDataModel, "CatalogEntityDataModel is required to exist in order for CommercePipelineExecutionContext to run").IsNotNull();
            Condition.Requires(entityDataModel.EntityId, "EntityId is reguired in input JSON data").IsNotNullOrEmpty();
            Condition.Requires(entityDataModel.CommerceEntityId, "Commerce Entity ID cannot be identified based on input JSON data").IsNotNullOrEmpty();
            entity = await _commerceCommander.Command<FindEntityCommand>().Process(context.CommerceContext, typeof(CommerceEntity), entityDataModel.CommerceEntityId);
            if (entity == null)
            {
                var errorMessage = $"Error: Commerce Entity with ID={entityDataModel.EntityId} not found, UpdateComposerFieldsBlock cannot be executed.";
                Log.Error(errorMessage);
                context.Abort(errorMessage, this);
                return arg;
            }


            var variant = ((SellableItem)entity).GetVariation(entityDataModel.EntityId);


            foreach (var component in entityDataModel.CustomComponentFields )
            {
                var type = Type.GetType(component.ComponentType);
                var method = typeof(ItemVariationComponent).GetMethod(nameof(ItemVariationComponent.GetComponent));
                if (method != null)
                {
                    var generic = method.MakeGenericMethod(type);
                    if (generic != null)
                    {
                        var c = generic.Invoke(variant, null);

                        if (c == null)
                        {
                            c = Activator.CreateInstance(type);
                            variant.SetComponent(c as Component);
                        }

                        foreach (var field in component.Fields)
                        {
                            var propertyInfo = type.GetProperty(field.Key);
                            if (propertyInfo != null)
                            {
                                if (propertyInfo.PropertyType.IsGenericType && propertyInfo.PropertyType.GetGenericTypeDefinition() == typeof(IList<>))
                                {
                                    var genericType = propertyInfo.PropertyType.GetGenericArguments().Single();
                                    var listType = typeof(List<>);
                                    var constructedListType = listType.MakeGenericType(genericType);

                                    var instance = Activator.CreateInstance(constructedListType);
                                    var list = (IList)instance;
                                    list.Add(Convert.ChangeType(field.Value, genericType));

                                    propertyInfo.SetValue(c, list, null);
                                }
                                else
                                {
                                    propertyInfo.SetValue(c, Convert.ChangeType(field.Value, propertyInfo.PropertyType),
                                        null);
                                }
                            }
                        }
                    }
                }
            }

            var persistResult = await _commerceCommander.Pipeline<IPersistEntityPipeline>().RunAsync(new PersistEntityArgument(entity), context);

            return arg;
        }
        #endregion

    }
}