using Sitecore.Commerce.Core;

namespace Plugin.Sync.Commerce.CatalogImport.Entities
{
    public class ImportCommerceEntityResponse
    {
        public CommerceEntity CommerceEntity { get; set; }
        public int StatusCode { get; set; }
        public bool IsNew { get; internal set; }
        public string ErrorMessage { get; set; }
    }
}