using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plugin.Sync.Commerce.CatalogImport.Models
{
    public class CatalogEntityDataBase
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public string Description { get; set; }
        public string CatalogName { get; set; }
        public string ParentCategoryName { get; set; }
        public Dictionary<string, string> EntityFields { get; set; }
        public List<string> ComposerFieldsRootPaths { get; set; }
        public Dictionary<string, string> ComposerFields { get; set; }
        public List<string> CustomFieldsRootPaths { get; set; }
        public Dictionary<string, string> CustomFields { get; set; }
        
    }
}
