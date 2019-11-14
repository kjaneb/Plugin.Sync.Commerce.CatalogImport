using Sitecore.Commerce.Core;
using Sitecore.Commerce.Plugin.Catalog;

namespace Plugin.Sync.Commerce.CatalogImport.Entities
{
    public class ImportSellableItemResponse
    {
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }
    }
}