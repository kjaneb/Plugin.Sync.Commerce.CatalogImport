using Newtonsoft.Json.Linq;
using Plugin.Sync.Commerce.CatalogImport.Policies;
using Sitecore.Commerce.Core;

namespace Plugin.Sync.Commerce.CatalogImport.Pipelines.Arguments
{
    public class ImportCatalogEntityArgument : PipelineArgument
    {
        public ImportCatalogEntityArgument(JObject request, MappingPolicyBase mappingPolicy)
        {
            Request = request;
            MappingPolicy = mappingPolicy;
        }
        public JObject Request { get; set; }
        public MappingPolicyBase MappingPolicy { get; set; }
    }
}
