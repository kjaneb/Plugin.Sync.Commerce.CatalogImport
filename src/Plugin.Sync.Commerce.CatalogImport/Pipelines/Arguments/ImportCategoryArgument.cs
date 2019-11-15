using Newtonsoft.Json.Linq;
using Plugin.Sync.Commerce.CatalogImport.Models;
using Sitecore.Commerce.Plugin.Catalog;

namespace Plugin.Sync.Commerce.CatalogImport.Pipelines.Arguments
{
    public class ImportCategoryArgument : ImportCatalogEntityArgumentBase
    {
        public ImportCategoryArgument(JObject jsonData) : base(jsonData)
        {
        }
        public CategoryEntityData EntityData { get; set; }
        public Category Category { get; set; }
    }
}