using Newtonsoft.Json.Linq;
using Sitecore.Framework.Conditions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Plugin.Sync.Commerce.CatalogImport.Models;
using Plugin.Sync.Commerce.CatalogImport.Policies;

namespace Plugin.Sync.Commerce.CatalogImport.Extensions
{
    public static class JsonExtensions
    {
        public static T SelectValue<T>(this JToken jObj, string jsonPath)
        {
            if (string.IsNullOrEmpty(jsonPath)) return default(T);

            var token = jObj.SelectToken(jsonPath);
            return token != null ? token.Value<T>() : default(T);
        }

        public static Dictionary<string, string> SelectMappedValues(this JToken jObj, Dictionary<string, string> mappings)
        {
            var fieldValues = new Dictionary<string, string>();
            if (mappings != null)
            {
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
            }

            return fieldValues;
        }

        public static List<CustomComponentModel> SelectMappedValues(this JToken jObj, List<CustomComponentPolicy> components)
        {
            var results = new List<CustomComponentModel>();

            foreach (var component in components)
            {
                var fieldValues = new Dictionary<string, string>();
                foreach (var key in component.Fields.Keys)
                {
                    if (!string.IsNullOrEmpty(component.Fields[key]))
                    {
                        var value = jObj.SelectValue<string>(component.Fields[key]);
                        if (!string.IsNullOrEmpty(value))
                        {
                            fieldValues.Add(key, value);
                        }
                    }
                }
                results.Add(new CustomComponentModel { ComponentType = component.ComponentType, Fields = fieldValues});
            }

            return results;
        }

        public static Dictionary<string, string> QueryMappedValuesFromRoot(this JToken jData, List<string> rootPaths)
        {
            var results = new Dictionary<string, string>();
            if (rootPaths != null)
            {
                foreach (var rootPath in rootPaths)
                {
                    if (!string.IsNullOrEmpty(rootPath))
                    {
                        var rootToken = jData.SelectToken(rootPath);
                        if (rootToken != null)
                        {
                            //var dict = rootToken.ToDictionary<string>();
                            foreach (var prop in rootToken.Children<JProperty>())
                            {
                                if (prop != null && !string.IsNullOrEmpty(prop.Name) && prop.Value != null && prop.Value.Type != JTokenType.Object)
                                {
                                    results.Add(prop.Name, (string)prop);
                                }
                            }
                        }
                    }
                }
            }
            return results;
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
