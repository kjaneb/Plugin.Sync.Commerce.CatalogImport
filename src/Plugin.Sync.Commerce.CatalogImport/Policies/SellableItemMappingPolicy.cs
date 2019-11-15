using Sitecore.Commerce.Core;
using System.Collections.Generic;

namespace Plugin.Sync.Commerce.CatalogImport.Policies
{
    public class SellableItemMappingPolicy : MappingPolicyBase
    {
        public string BrandPath { get; set; }
        public string ManufacturerPath { get; set; }
        public string TypeOfGoodsPath { get; set; }
        public string ListPricePath { get; set; } 
    }
}

