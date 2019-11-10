using Sitecore.Commerce.Core;

namespace Plugin.Sync.Commerce.CatalogExport.Policies
{
    public class CatalogExportClienPolicy : Policy
    {
        public string BaseUrl { get; set; }
        public string ApplicationId { get; set; }
        public string SharedSecret { get; set; }
        public string RefreshToken { get; set; }
        public string ProfileId { get; set; }
    }
}
