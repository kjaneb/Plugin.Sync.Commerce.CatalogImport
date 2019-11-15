using Newtonsoft.Json.Linq;
using Sitecore.Commerce.Core;

namespace Plugin.Sync.Commerce.CatalogImport.Pipelines.Arguments
{
    public class ImportCatalogEntityArgument : PipelineArgument
    {
        public ImportCatalogEntityArgument(JObject request)
        {
            Request = request;
        }
        public JObject Request { get; set; }
    }
}
