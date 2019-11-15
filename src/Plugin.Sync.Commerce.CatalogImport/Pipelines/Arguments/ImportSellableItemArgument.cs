using Newtonsoft.Json.Linq;
using Plugin.Sync.Commerce.CatalogImport.Models;
using Sitecore.Commerce.Plugin.Catalog;

namespace Plugin.Sync.Commerce.CatalogImport.Pipelines.Arguments
{
    public class ImportSellableItemArgument : ImportCatalogEntityArgumentBase
    {
        public ImportSellableItemArgument(JObject jsonData) : base(jsonData)
        {
        }

        public SellableItemEntityData EntityData { get; set; }
        public SellableItem SellableItem { get; set; }
    }
}