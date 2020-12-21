using System.Collections.Generic;
using Sitecore.Commerce.Core;

namespace Plugin.Sync.Commerce.CatalogImport.Models
{
    public class CustomComponentModel : Model
    {
        public string ComponentType { get; set; }
        public Dictionary<string, string> Fields { get; set; }
    }
}
