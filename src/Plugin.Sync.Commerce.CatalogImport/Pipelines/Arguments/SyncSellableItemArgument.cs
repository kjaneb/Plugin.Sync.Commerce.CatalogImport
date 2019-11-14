using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Plugin.Sync.Commerce.CatalogImport.Entities;
using Plugin.Sync.Commerce.CatalogImport.Models;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.Plugin.Catalog;
using Sitecore.Framework.Conditions;

namespace Plugin.Sync.Commerce.CatalogImport.Pipelines.Arguments
{
    public class ImportSellableItemArgument : PipelineArgument
    {
        public ImportSellableItemArgument(JObject jsonData)
        {
            Condition.Requires<JObject>(jsonData).IsNotNull("jsonData can not be null");
            this.JsonData = jsonData;
        }
        
        public JObject JsonData { get; set; }
        public CatalogEntityData EntityData { get; set; }
        public SellableItem SellableItem { get; set; }
    }
}