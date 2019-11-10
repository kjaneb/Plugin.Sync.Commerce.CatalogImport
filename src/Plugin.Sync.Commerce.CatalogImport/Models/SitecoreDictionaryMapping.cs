using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plugin.Sync.Commerce.CatalogImport.Models
{
    public class SitecoreDictionaryMapping
    {
        public string ItemName { get; set; }
        public string FromValue { get; set; }
        public string ToValue { get; set; }
        public bool SubstringLookup { get; set; }
    }
}
