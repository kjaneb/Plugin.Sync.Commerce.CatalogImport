using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Plugin.Sync.Commerce.CatalogImport.Extensions
{
    public static class DictionaryExtensions
    {
        public static void AddRange<T, S>(this IDictionary<T, S> source, IDictionary<T, S> collection)
        {
            if (collection == null)
            {
                throw new ArgumentNullException("Empty dictionary");
            }

            foreach (var item in collection)
            {
                if (!source.ContainsKey(item.Key))
                {
                    source.Add(item.Key, item.Value);
                }
            }
        }

        public static IDictionary<string, object> ToDictionary(this object source)
        {
            return source.ToDictionary<object>();
        }

        public static IDictionary<string, T> ToDictionary<T>(this object source)
        {
            if (source == null) ThrowExceptionWhenSourceArgumentIsNull();
            var comparer = StringComparer.OrdinalIgnoreCase;
            var dictionary = new Dictionary<string, T>(comparer);
            foreach (PropertyDescriptor property in TypeDescriptor.GetProperties(source))
            {
                object value = property.GetValue(source);
                if (IsOfType<T>(value))
                {
                    dictionary.Add(property.Name, (T)value);
                }
                else if (IsOfTypeListOfStrings(value))
                {
                    var arr = ((IEnumerable)value).Cast<object>().Select(x => x.ToString()).ToArray();
                    object convertedValue = string.Join(",", arr);
                    dictionary.Add(property.Name, (T)convertedValue);
                }
            }
            return dictionary;
        }

        private static bool IsOfTypeListOfStrings(object value)
        {
            return value is List<string>;
        }

        private static bool IsOfType<T>(object value)
        {
            return value is T;
        }

        private static void ThrowExceptionWhenSourceArgumentIsNull()
        {
            throw new NullReferenceException("Unable to convert anonymous object to a dictionary. The source anonymous object is null.");
        }
    }
}