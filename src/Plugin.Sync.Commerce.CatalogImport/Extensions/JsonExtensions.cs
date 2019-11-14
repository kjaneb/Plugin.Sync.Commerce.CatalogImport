using Newtonsoft.Json.Linq;
using Sitecore.Framework.Conditions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plugin.Sync.Commerce.CatalogImport.Extensions
{
    public static class JsonExtensions
    {
        public static T SelectValue<T>(this JToken jObj, string jsonPath)
        {
            Condition.Requires(jsonPath, nameof(jsonPath)).IsNotNullOrEmpty();
            var token = jObj.SelectToken(jsonPath);
            return token != null ? token.Value<T>() : default(T);
        }

        public static Dictionary<string, string> SelectMappedValues(this JToken jObj, Dictionary<string, string> mappings)
        {
            var fieldValues = new Dictionary<string, string>();
            foreach (var key in mappings.Keys)
            {
                if (!string.IsNullOrEmpty(mappings[key]))
                {
                    var value = jObj.SelectValue<string>(mappings[key]);
                    if (!string.IsNullOrEmpty(value))
                    {
                        fieldValues.Add(key, value);
                    }
                }
            }

            return fieldValues;
        }

        public static T QueryMappedValue<T>(this JToken jData, string fieldName, string fieldPath, IEnumerable<string> rootPaths)
        {
            if (!string.IsNullOrEmpty(fieldName) && !string.IsNullOrEmpty(fieldPath))
            {
                return jData.SelectValue<T>(fieldPath);
            }

            foreach (var rootPath in rootPaths)
            {
                if (!string.IsNullOrEmpty(rootPath))
                {
                    var rootToken = jData.SelectToken(rootPath);
                    if (rootToken != null)
                    {
                        return rootToken.SelectValue<T>(rootPath);
                    }
                }
            }

            return default(T);
        }
    }
}
