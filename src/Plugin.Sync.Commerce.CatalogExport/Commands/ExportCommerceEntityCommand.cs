using Plugin.Sync.Commerce.CatalogExport.Pipelines;
using Plugin.Sync.Commerce.CatalogExport.Pipelines.Arguments;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.Core.Commands;
using System;
using System.Threading.Tasks;

namespace Plugin.Sync.Commerce.CatalogImport.Commands
{
    public class ExportCommerceEntityCommand : CommerceCommand
    {
        private readonly IExportCommerceEntityPipeline _pipeline;

        public ExportCommerceEntityCommand(IExportCommerceEntityPipeline pipeline, IServiceProvider serviceProvider) : base(serviceProvider)
        {
            this._pipeline = pipeline;
        }

        public async Task<ExportCommerceEntityArgument> Process(CommerceContext commerceContext, ExportCommerceEntityArgument args)
        {
            using (var activity = CommandActivity.Start(commerceContext, this))
            {
                var result = await this._pipeline.Run(args, new CommercePipelineExecutionContextOptions(commerceContext));
                return result;
            }
        }
    }
}