﻿using System.Text.Json;
using Newtonsoft.Json.Linq;
using Plugin.Sync.Commerce.CatalogImport.Extensions;
using Plugin.Sync.Commerce.CatalogImport.Models;
using Plugin.Sync.Commerce.CatalogImport.Pipelines.Arguments;
using Plugin.Sync.Commerce.CatalogImport.Policies;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.Plugin.Catalog;
using Sitecore.Framework.Conditions;
using Sitecore.Framework.Pipelines;
using System.Threading.Tasks;

namespace Plugin.Sync.Commerce.CatalogImport.Pipelines.Blocks
{
    /// <summary>
    /// Extract Commerce Entity fields from input JSON using Entity's MappingPolicy to find matching fields in input JSON
    /// </summary>
    [PipelineDisplayName("ExtractCatalogEntityFieldsFromJsonDataBlock")]
    public class ExtractCatalogEntityFieldsFromJsonDataBlock : AsyncPipelineBlock<ImportCatalogEntityArgument, ImportCatalogEntityArgument, CommercePipelineExecutionContext>
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
        public override async Task<ImportCatalogEntityArgument> RunAsync(ImportCatalogEntityArgument arg,
            CommercePipelineExecutionContext context)
        {
            var mappingPolicy = arg.MappingPolicy;

            var jsonData = arg.Request as JObject;
            Condition.Requires(jsonData, "Commerce Entity JSON parameter is required").IsNotNull();
            context.AddModel(new JsonDataModel(jsonData));

            var entityDataModel = context.GetModel<CatalogEntityDataModel>();
            var entityData = new CatalogEntityDataModel
            {
                EntityId = jsonData.SelectValue<string>(mappingPolicy.EntityId),
                EntityName = jsonData.SelectValue<string>(mappingPolicy.EntityName),
                ParentCatalogName = jsonData.SelectValue<string>(mappingPolicy.ParentCatalogName),
                ParentCategoryName = jsonData.SelectValue<string>(mappingPolicy.ParentCategoryName),
                EntityFields = jsonData.SelectMappedValues(mappingPolicy.EntityFieldsPaths),
                ComposerFields = jsonData.SelectMappedValues(mappingPolicy.ComposerFieldsPaths),
                CustomFields = jsonData.SelectMappedValues(mappingPolicy.CustomFieldsPaths)
            };

            if (mappingPolicy is SellableItemMappingPolicy)
            {
                entityData.CustomComponentFields =
                    jsonData.SelectMappedValues(((SellableItemMappingPolicy) mappingPolicy).CustomComponentPaths);
            }

            if (mappingPolicy is VariantMappingPolicy)
            {
                entityData.ParentProductName =
                    jsonData.SelectValue<string>(((VariantMappingPolicy) mappingPolicy).ParentProductName);
                entityData.CustomComponentFields =
                    jsonData.SelectMappedValues(((VariantMappingPolicy)mappingPolicy).CustomComponentPaths);
            }

            entityData.EntityFields.AddRange(jsonData.QueryMappedValuesFromRoot(mappingPolicy.EntityFieldsRootPaths));
            entityData.ComposerFields.AddRange(jsonData.QueryMappedValuesFromRoot(mappingPolicy.ComposerFieldsRootPaths));
            //entityData.CustomFields.AddRange(jsonData.QueryMappedValuesFromRoot(mappingPolicy.CustomFieldsRootPaths));

            if (string.IsNullOrEmpty(entityData.ParentCatalogName))
            {
                entityData.ParentCatalogName = mappingPolicy.DefaultCatalogName;
            }

            if (string.IsNullOrEmpty(entityData.ParentCategoryName))
            {
                entityData.ParentCategoryName = mappingPolicy.DefaultCategoryName;
            }

            if (arg.CommerceEntityType != null && !string.IsNullOrEmpty(entityData.EntityName))
            {
                if (arg.CommerceEntityType == typeof(Category) && !string.IsNullOrEmpty(entityData.ParentCatalogName))
                {
                    entityData.CommerceEntityId = $"{CommerceEntity.IdPrefix<Category>()}{entityData.ParentCatalogName}-{entityData.EntityName}";
                }
                else if (mappingPolicy is VariantMappingPolicy)
                {
                    entityData.CommerceEntityId = $"{CommerceEntity.IdPrefix<SellableItem>()}{entityData.ParentProductName}";
                }
                else
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
