using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plugin.Sync.Commerce.CatalogImport.Models
{
    public class GetSyncData
    {

        public static string GetSyncVal(string jsonValue, bool doNotOverwrite, bool mappingFound, string fromValue, string toValue)
        {
            var output = jsonValue;

            switch (doNotOverwrite)
            {
                case true:

                    output = jsonValue;
                    break;

                case false:
                    if (mappingFound && !string.IsNullOrEmpty(fromValue) && !string.IsNullOrEmpty(toValue))
                    {
                        output = toValue;
                    }
                    break;
            }


            return output;
        }
    }
}

