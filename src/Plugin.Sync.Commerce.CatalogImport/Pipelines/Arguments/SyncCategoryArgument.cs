using System.Collections.Generic;
using Sitecore.Commerce.Core;
using Sitecore.Framework.Conditions;

namespace Plugin.Sync.Commerce.CatalogImport.Pipelines.Arguments
{
    public class ImportCategoryArgument : PipelineArgument
    {
        public ImportCategoryArgument(Dictionary<string, string> features)
        {
            Condition.Requires<string>(features["CatalogId"]).IsNotNullOrEmpty("The catalog name can not be null");
            Condition.Requires<string>(features["CategoryId"]).IsNotNullOrEmpty("The category name can not be null");
            Condition.Requires<string>(features["DisplayName"]).IsNotNullOrEmpty("The display name can not be null");
            this.Features = features;
        }
        public Dictionary<string, string> Features { get; set; }
    }
}