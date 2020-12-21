using Newtonsoft.Json.Linq;
using Plugin.Sync.Commerce.CatalogImport.Policies;
using Sitecore.Commerce.Core;
using System;
using System.Text.Json;

namespace Plugin.Sync.Commerce.CatalogImport.Pipelines.Arguments
{
    public class ImportCatalogEntityArgument : PipelineArgument
    {
        public ImportCatalogEntityArgument(JObject request, MappingPolicyBase mappingPolicy, Type commerceEntityType)
        {
            Request = request;
            MappingPolicy = mappingPolicy;
            CommerceEntityType = commerceEntityType;
        }

        public ImportCatalogEntityArgument(MappingPolicyBase mappingPolicy, Type commerceEntityType)
        {
            MappingPolicy = mappingPolicy;
            CommerceEntityType = commerceEntityType;
        }

        public string ContentHubEntityId { get; set; }
        public JObject Request { get; set; }
        public MappingPolicyBase MappingPolicy { get; set; }
        public Type CommerceEntityType { get; set; }
    }
}
