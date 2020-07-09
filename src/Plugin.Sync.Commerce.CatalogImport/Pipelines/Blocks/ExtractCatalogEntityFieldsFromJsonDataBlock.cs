using Newtonsoft.Json.Linq;
using Plugin.Sync.Commerce.CatalogImport.Extensions;
using Plugin.Sync.Commerce.CatalogImport.Models;
using Plugin.Sync.Commerce.CatalogImport.Pipelines.Arguments;
using Plugin.Sync.Commerce.CatalogImport.Policies;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.Plugin.Catalog;
using Sitecore.Data.Clones.ItemSourceUriProviders;
using Sitecore.Data.Comparers;
using Sitecore.Framework.Conditions;
using Sitecore.Framework.Pipelines;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Plugin.Sync.Commerce.CatalogImport.Pipelines.Blocks
{
    /// <summary>
    /// Extract Commerce Entity fields from input JSON using Entity's MappingPolicy to find matching fields in input JSON
    /// </summary>
    [PipelineDisplayName("ExtractCatalogEntityFieldsFromJsonDataBlock")]
    public class ExtractCatalogEntityFieldsFromJsonDataBlock : PipelineBlock<ImportCatalogEntityArgument, ImportCatalogEntityArgument, CommercePipelineExecutionContext>
    {
        public ExtractCatalogEntityFieldsFromJsonDataBlock()
        {
        }

        /// <summary>
        /// Main execution point
        /// </summary>
        /// <param name="arg"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public override async Task<ImportCatalogEntityArgument> Run(ImportCatalogEntityArgument arg, CommercePipelineExecutionContext context)
        {
            var mappingPolicy = arg.MappingPolicy;

            var jsonData = arg.Entity as JObject;
            Condition.Requires(jsonData, "Commerce Entity JSON parameter is required").IsNotNull();
            context.AddModel(new JsonDataModel(jsonData));

            var entityDataModel = context.GetModel<CatalogEntityDataModel>();

            var rootEntityFields = mappingPolicy.FieldPaths.Where(s => !arg.RelatedEntities.ContainsKey(s.Key)).ToDictionary(k => k.Key, v => v.Value);

            var entityData = new CatalogEntityDataModel
            {
                EntityId = jsonData.SelectValue<string>(mappingPolicy.EntityId),
                EntityName = jsonData.SelectValue<string>(mappingPolicy.EntityName),
                ParentCatalogName = jsonData.SelectValue<string>(mappingPolicy.ParentCatalogName),
                EntityFields = jsonData.SelectMappedValues(rootEntityFields),
            };

            var refEntityFields = mappingPolicy.FieldPaths.Where(s => arg.RelatedEntities.ContainsKey(s.Key)).ToDictionary(k => k.Key, v => v.Value);
            if (refEntityFields != null && refEntityFields.Count > 0 && arg.RelatedEntities != null && arg.RelatedEntities.Count > 0)
            {
                foreach (var key in arg.RelatedEntities.Keys)
                {
                    if (arg.RelatedEntities[key] != null && refEntityFields.ContainsKey(key))
                    {
                        var fieldValues = new List<string>();
                        foreach (var refEntity in arg.RelatedEntities[key])
                        {
                            if (refEntity != null)
                            {
                                var fieldValue = refEntity.SelectValue<string>(refEntityFields[key]);
                                if (fieldValue != null)
                                {
                                    fieldValues.Add(fieldValue);
                                }
                            }
                        }
                        if (fieldValues.Any())
                        {
                            entityData.EntityFields.Add(key, string.Join("|", fieldValues));
                        }
                    }
                }
            }

            if (string.IsNullOrEmpty(entityData.ParentCatalogName))
            {
                entityData.ParentCatalogName = mappingPolicy.DefaultCatalogName;
            }

            if (!string.IsNullOrEmpty(mappingPolicy.ListPricePath))
            {
                var price = jsonData.SelectValue<string>(mappingPolicy.ListPricePath);
                if (!string.IsNullOrEmpty(price))
                {
                    decimal parcedPrice;
                    if (decimal.TryParse(price, out parcedPrice))
                    {
                        entityData.ListPrice = parcedPrice;
                    }
                }
            }

            arg.ParentEntityIds = new List<string>();
            if (arg.ParentRelationsEntity != null && !string.IsNullOrEmpty(mappingPolicy.ParentRelationParentsPath))
            {
                var parentTokens = arg.ParentRelationsEntity.SelectTokens(mappingPolicy.ParentRelationParentsPath);
                if (parentTokens != null )
                {
                    foreach (JToken parentToken in parentTokens)
                    {
                        var parentUrl = parentToken.Value<string>();
                        if (!string.IsNullOrEmpty(parentUrl))
                        {
                            var parentEntityId = parentUrl.Split('/').LastOrDefault();
                            if (long.TryParse(parentEntityId, out long value))
                            {
                                arg.ParentEntityIds.Add(parentEntityId);
                            }
                        }
                    }
                }
            }

            if (arg.CommerceEntityType != null && !string.IsNullOrEmpty(entityData.EntityName))
            {
                if (arg.CommerceEntityType == typeof(Category) && !string.IsNullOrEmpty(entityData.ParentCatalogName))
                {
                    entityData.CommerceEntityId = $"{CommerceEntity.IdPrefix<Category>()}{entityData.ParentCatalogName}-{entityData.EntityName}";
                }
                else if (arg.CommerceEntityType == typeof(SellableItem))
                {
                    entityData.CommerceEntityId = $"{CommerceEntity.IdPrefix<SellableItem>()}{entityData.EntityId}";
                }
            }

            context.AddModel(entityData);

            await Task.CompletedTask;

            return arg;
        }
    }
}
