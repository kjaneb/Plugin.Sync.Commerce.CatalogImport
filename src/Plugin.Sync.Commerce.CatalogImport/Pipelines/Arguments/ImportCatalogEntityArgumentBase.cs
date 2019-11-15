using Newtonsoft.Json.Linq;
using Plugin.Sync.Commerce.CatalogImport.Models;
using Sitecore.Commerce.Core;
using Sitecore.Framework.Conditions;

namespace Plugin.Sync.Commerce.CatalogImport.Pipelines.Arguments
{
    public class ImportCatalogEntityArgumentBase : PipelineArgument
    {
        public ImportCatalogEntityArgumentBase(JObject jsonData)
        {
            Condition.Requires<JObject>(jsonData).IsNotNull("jsonData can not be null");
            this.JsonData = jsonData;
        }
        public JObject JsonData { get; set; }
    }
}
