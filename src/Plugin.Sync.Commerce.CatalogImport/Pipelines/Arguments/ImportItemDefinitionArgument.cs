using System;
using System.Collections.Generic;
using System.Text;
using Sitecore.Commerce.Core;

namespace Plugin.Sync.Commerce.CatalogImport.Pipelines.Arguments
{
    public class ImportItemDefinitionArgument : PipelineArgument
    {
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public List<string> Fields { get; set; }
    }
}
