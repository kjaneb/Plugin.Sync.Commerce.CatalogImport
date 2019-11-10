using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plugin.Sync.Commerce.CatalogImport.Models
{
    public class SitecoreFieldMapping
    {
        public string ItemName { get; set; }
        public string FieldName { get; set; }
        public string ValueJsonPath { get; set; }
        public bool DoNotOverwrite { get; set; }
        public List<SitecoreDictionaryMapping> DictionaryMappings { get; set; }
    }
}
