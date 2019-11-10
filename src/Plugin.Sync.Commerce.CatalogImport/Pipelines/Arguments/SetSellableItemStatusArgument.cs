using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Plugin.Sync.Commerce.CatalogImport.Entities;
using Sitecore.Commerce.Core;
using Sitecore.Framework.Conditions;

namespace Plugin.Sync.Commerce.CatalogImport.Pipelines.Arguments
{
    public class SetSellableItemStatusArgument : PipelineArgument
    {
        public SetSellableItemStatusArgument(JObject jsonData)
        {
            this.Status = jsonData["Status"]?.ToString();
            this.Id = jsonData["Id"]?.ToString();
            
            Condition.Requires<string>(this.Id).IsNotNullOrEmpty("The sellable item(product) identifier can not be null");
            Condition.Requires<string>(this.Status).IsNotNullOrEmpty("The sellable status can not be null");
        }
        public string Status { get; set; }
        public string Id { get; set; }

    }
}