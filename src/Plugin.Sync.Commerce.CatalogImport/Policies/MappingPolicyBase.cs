using System;
using System.Collections.Generic;
using Sitecore.Commerce.Core;


namespace Plugin.Sync.Commerce.CatalogImport.Policies
{
    public class MappingPolicyBase: Policy
    {
        public bool ClearFieldValues { get; set; }
        public string EntityId { get; set; }
        public string EntityName { get; set; }
        public string ParentCategoryName { get; set; }
        public string ParentCatalogName { get; set; }
        public List<string> EntityFieldsRootPaths { get; set; }
        public Dictionary<string, string> EntityFieldsPaths { get; set; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        public List<string> ComposerFieldsRootPaths { get; set; }
        public Dictionary<string, string> ComposerFieldsPaths { get; set; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        public List<string> CustomFieldsRootPaths { get; set; }
        public Dictionary<string, string> CustomFieldsPaths { get; set; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        public string UpdatedItemsList { get; set; }
        public string DefaultCatalogName { get; set; }
        public string DefaultCategoryName { get; set; }

    }
}
