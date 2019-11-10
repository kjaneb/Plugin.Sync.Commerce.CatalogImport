using System.Collections.Generic;
using Sitecore.Commerce.Core;


namespace Plugin.Sync.Commerce.CatalogImport.Policies
{
    public class CatalogEntityMappingPolicy : Policy
    {
        public string IdPath { get; set; }
        public string NamePath { get; set; }
        public string DisplayNamePath { get; set; }
        public string DescriptionPath { get; set; }
        public string ParentCategoryNamePath { get; set; }
        public string CatalogNamePath { get; set; }
        public List<string> RootPaths { get; set; }
        public Dictionary<string, string> FieldPaths { get; set; }
        public string UpdatedItemsList { get; set; }
        public string CreatedItemsList { get; set; }
    }
}
