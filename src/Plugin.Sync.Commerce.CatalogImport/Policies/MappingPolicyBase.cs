using System;
using System.Collections.Generic;
using Sitecore.Commerce.Core;


namespace Plugin.Sync.Commerce.CatalogImport.Policies
{
    public class MappingPolicyBase : Policy
    {
        public string EntityId { get; set; }
        public string EntityName { get; set; }
        public string ParentCatalogName { get; set; }
        public Dictionary<string, string> FieldPaths { get; set; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        public Dictionary<string, List<string>> RelatedEntityPaths { get; set; } = new Dictionary<string, List<string>> (StringComparer.OrdinalIgnoreCase);
        public string SyncedItemsList { get; set; }
        public string DefaultCatalogName { get; set; }
        public string DefaultCategoryName { get; set; }
        public string ListPricePath { get; set; }
        public string ParentRelationEntityPath { get; set; }
        public string ParentRelationParentsPath { get; set; }

    }
}
