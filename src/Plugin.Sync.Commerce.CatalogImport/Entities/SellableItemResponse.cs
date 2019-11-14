using Sitecore.Commerce.Core;
using Sitecore.Commerce.Plugin.Catalog;

namespace Plugin.Sync.Commerce.CatalogImport.Entities
{
    public class ImportSellableItemResponse
    {
        public SellableItem SellableItem { get; set; }
        public int StatusCode { get; set; }
        public bool IsNew { get; internal set; }
        public string ErrorMessage { get; set; }
    }
}