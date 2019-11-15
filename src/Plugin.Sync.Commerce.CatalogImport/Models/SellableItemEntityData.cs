using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plugin.Sync.Commerce.CatalogImport.Models
{
    public class SellableItemEntityData: CatalogEntityDataBase
    {
       
        public string Brand { get; set; }
        public string Manufacturer { get; set; }
        public string TypeOfGoods { get; set; }
    }
}
