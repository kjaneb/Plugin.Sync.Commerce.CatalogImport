using Sitecore.Commerce.Core;
using Sitecore.Commerce.Plugin.Catalog;
using Sitecore.Commerce.Plugin.Composer;
using System.Collections.Generic;

namespace Plugin.Sync.Commerce.CatalogImport.Helpers
{
    public class CommerceCatalogHelper
    {
        public static string GetCommerceCategoryId(string catalogName, string categoryId)
        {
            return $"{CommerceEntity.IdPrefix<Category>()}{catalogName}-{categoryId}";
        }

        public static string GetCommerceSellableItemId(string entityId)
        {
            return $"{CommerceEntity.IdPrefix<SellableItem>()}{entityId}";
        }

        public static string GetCommerceCatalogId(string catalogName)
        {
            return $"{CommerceEntity.IdPrefix<Catalog>()}{catalogName}";
        }

        public static decimal GetListPriceFromPriceCollection(Dictionary<string, decimal> prices)
        {
            if (prices == null || prices.Count < 1 || !prices.ContainsKey("ListPrice"))
            {
                return 0;
            }
            return prices["ListPrice"];
        }
        public static IDictionary<string, string> GetComposerDisctionary(SellableItem sellableItem)
        {
            var dictionary = new Dictionary<string, string>();

            var composerTemplateViewComponents = sellableItem.GetComponent<ComposerTemplateViewsComponent>().Views;
            foreach (var view in composerTemplateViewComponents)
            {
                var composerView = sellableItem.GetComposerView(view.Key);
                foreach (var property in composerView.Properties)
                {
                    if (!dictionary.ContainsKey(property.Name))
                    {
                        dictionary.Add(property.Name, property.RawValue != null ? property.RawValue.ToString() : string.Empty);
                    }
                    else
                    {
                        dictionary[property.Name] = property.RawValue != null ? property.RawValue.ToString() : string.Empty;
                    }
                }
            }
            return dictionary;
        }
    }
}