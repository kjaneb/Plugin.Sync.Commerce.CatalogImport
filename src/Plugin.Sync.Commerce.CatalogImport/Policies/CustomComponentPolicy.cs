using System.Collections.Generic;
using Sitecore.Commerce.Core;

namespace Plugin.Sync.Commerce.CatalogImport.Policies
{
    public class CustomComponentPolicy : Policy
    {
        public string ComponentType { get; set; }
        public Dictionary<string, string> Fields { get; set; }
    }
}
