using Newtonsoft.Json.Linq;
using Plugin.Sync.Commerce.CatalogImport.Extensions;
using Plugin.Sync.Commerce.CatalogImport.Models;
using Plugin.Sync.Commerce.CatalogImport.Pipelines.Arguments;
using Plugin.Sync.Commerce.CatalogImport.Policies;
using Sitecore.Commerce.Core;
using Sitecore.Framework.Conditions;
using Sitecore.Framework.Pipelines;
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

            //var sellableItemMappingPolicy = context.CommerceContext.GetPolicy<SellableItemMappingPolicy>();
            //var categoryMappingPolicy = context.CommerceContext.GetPolicy<CategoryMappingPolicy>();

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
                CustomFields = jsonData.SelectMappedValues(mappingPolicy.CustomFieldsPaths),
            };

            entityData.EntityFields.AddRange(jsonData.QueryMappedValuesFromRoot(mappingPolicy.EntityFieldsRootPaths));
            entityData.ComposerFields.AddRange(jsonData.QueryMappedValuesFromRoot(mappingPolicy.ComposerFieldsRootPaths));
            entityData.CustomFields.AddRange(jsonData.QueryMappedValuesFromRoot(mappingPolicy.CustomFieldsRootPaths));

            if (string.IsNullOrEmpty(entityData.ParentCatalogName))
            {
                entityData.ParentCatalogName = mappingPolicy.DefaultCatalogName;
            }

            if (string.IsNullOrEmpty(entityData.ParentCategoryName))
            {
                entityData.ParentCatalogName = mappingPolicy.DefaultCategoryName;
            }

            context.AddModel(entityData);

            await Task.CompletedTask;

            return arg;
        }
    }
}
