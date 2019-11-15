using Sitecore.Commerce.Core;
using System;
using System.Collections.Generic;

namespace Plugin.Sync.Commerce.CatalogImport.Models
{
    public class CatalogEntityDataModel: Model
    {
        public string EntityId { get; set; }
        public string EntityName { get; set; }
        public string ParentCatalogName { get; set; }
        public string ParentCategoryName { get; set; }
        public Dictionary<string, string> EntityFields { get; set; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        public Dictionary<string, string> ComposerFields { get; set; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        public Dictionary<string, string> CustomFields { get; set; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

    }
}
