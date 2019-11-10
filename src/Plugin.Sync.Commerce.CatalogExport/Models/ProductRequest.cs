using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plugin.Sync.Commerce.CatalogExport.Models
{
    public class ProductRequest
    {
        public int ID { get; set; }
        public string ProfileID { get; set; }
        public string Sku { get; set; }
        public string Title { get; set; }
        public string Manufacturer { get; set; }
        public string Description { get; set; }
        public string Brand { get; set; }
        public decimal RetailPrice { get; set; }
    }
}
