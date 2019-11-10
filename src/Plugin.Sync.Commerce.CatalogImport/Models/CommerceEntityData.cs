using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plugin.Sync.Commerce.CatalogImport.Models
{
    public class CommerceEntityData
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public string Description { get; set; }
        public string CatalogName { get; set; }
        public string ParentCategoryName { get; set; }
        public Dictionary<string, string> Fields { get; set; }
    }
}
