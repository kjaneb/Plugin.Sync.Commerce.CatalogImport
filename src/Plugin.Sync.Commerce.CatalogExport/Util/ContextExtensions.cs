using Plugin.Sync.Commerce.CatalogExport.Pipelines.Arguments;
using Sitecore.Commerce.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plugin.Ryder.Commerce.CatalogExport.Util
{
    public static class ContextExtensions
    {
        public static ExportCommerceEntityArgument AbortPipeline(this CommercePipelineExecutionContext context, ExportCommerceEntityArgument arg, string message)
        {
            arg.Success = false;
            arg.ErrorMessage = message;
            context.Abort(message, arg);
            return arg;
        }
    }
}
