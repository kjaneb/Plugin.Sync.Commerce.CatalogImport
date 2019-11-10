using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Plugin.Sync.Commerce.CatalogImport.Entities;
using Plugin.Sync.Commerce.CatalogImport.Models;
using Sitecore.Commerce.Core;
using Sitecore.Framework.Conditions;

namespace Plugin.Sync.Commerce.CatalogImport.Pipelines.Arguments
{
    public class ImportCommerceEntityArgument : PipelineArgument
    {
        public ImportCommerceEntityArgument(JObject jsonData)
        {
            Condition.Requires<JObject>(jsonData).IsNull("jsonData can not be null");
            this.JsonData = jsonData;
        }
        
        public JObject JsonData { get; set; }
    }
}