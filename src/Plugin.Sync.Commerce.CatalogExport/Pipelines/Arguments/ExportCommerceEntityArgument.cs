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
        public ExportCommerceEntityArgument(string entitId, string viewName)
        {
            this.EntityId = entitId;
            this.ViewName = viewName;
        }
        public string EntityId { get; set; }
        public bool Success { get; set; } = true;
        public bool EntityNotFound { get; set; }
        public string ViewName { get; set; }
        public string Response { get; set; }
        public string ErrorMessage { get; set; }
        public string ViewTemplate { get; set; }
    }
}
