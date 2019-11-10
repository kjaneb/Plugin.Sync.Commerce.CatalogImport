using Plugin.Sync.Commerce.CatalogImport.Policies;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.Plugin.Catalog;
using System.Net;

namespace Plugin.Sync.Commerce.CatalogExport.Pipelines.Arguments
{
    public class CatalogExportArgument : PipelineArgument
    {
        public CatalogExportArgument(SellableItem sellableItem)
        {
            this.CurrentItem = sellableItem;
        }
        public SellableItem CurrentItem { get; set; }
        public SellableItemMappingPolicy SellableItemMappingPolicy { get; set; }
        public string ProfileId { get; set; }
        public string ResponseContent { get; set; }
        public bool IsSuccesful { get; set; }
        public HttpStatusCode ResponseStatusCode {get; set;}
        public string ErrorMessage { get; set; }
        public bool IsAlreadyDeleted { get; set; }
    }
}