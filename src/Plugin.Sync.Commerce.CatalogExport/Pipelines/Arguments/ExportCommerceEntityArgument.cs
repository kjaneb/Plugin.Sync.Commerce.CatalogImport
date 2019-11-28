using Sitecore.Commerce.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plugin.Sync.Commerce.CatalogExport.Pipelines.Arguments
{
    public class ExportCommerceEntityArgument : PipelineArgument
    {
        public ExportCommerceEntityArgument(string entitId)
        {
            this.EntityId = entitId;
        }
        public string EntityId { get; set; }
    }
}
