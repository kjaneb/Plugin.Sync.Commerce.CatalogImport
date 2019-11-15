using Plugin.Sync.Commerce.CatalogImport.Extensions;
using Plugin.Sync.Commerce.CatalogImport.Models;
using Plugin.Sync.Commerce.CatalogImport.Pipelines.Arguments;
using Plugin.Sync.Commerce.CatalogImport.Policies;
using Sitecore.Commerce.Core;
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

            var mappingPolicy = context.CommerceContext.GetPolicy<CatalogEntityMappingPolicy>();
            var jsonDataModel = context.GetModel<JsonDataModel>();
            var entityDataModel = context.GetModel<CatalogEntityDataModel>();
            var entityData = new CatalogEntityDataModel
            {
                EntityId = jsonDataModel.JsonData.SelectValue<string>(mappingPolicy.EntityId),
                EntityName = jsonDataModel.JsonData.SelectValue<string>(mappingPolicy.EntityName),
                ParentCatalogName = jsonDataModel.JsonData.SelectValue<string>(mappingPolicy.ParentCatalogName),
                ParentCategoryName = jsonDataModel.JsonData.SelectValue<string>(mappingPolicy.ParentCategoryName),
                EntityFields = jsonDataModel.JsonData.SelectMappedValues(mappingPolicy.EntityFieldsPaths),
                ComposerFields = jsonDataModel.JsonData.SelectMappedValues(mappingPolicy.ComposerFieldsPaths),
                CustomFields = jsonDataModel.JsonData.SelectMappedValues(mappingPolicy.CustomFieldsPaths),
            };

            entityData.EntityFields.AddRange(jsonDataModel.JsonData.QueryMappedValuesFromRoot(mappingPolicy.EntityFieldsRootPaths));
            entityData.ComposerFields.AddRange(jsonDataModel.JsonData.QueryMappedValuesFromRoot(mappingPolicy.ComposerFieldsRootPaths));
            entityData.CustomFields.AddRange(jsonDataModel.JsonData.QueryMappedValuesFromRoot(mappingPolicy.CustomFieldsRootPaths));

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
