using System;
using System.Threading.Tasks;
using Plugin.Sync.Commerce.CatalogImport.Entities;
using Plugin.Sync.Commerce.CatalogImport.Pipelines;
using Plugin.Sync.Commerce.CatalogImport.Pipelines.Arguments;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.Core.Commands;

namespace Plugin.Sync.Commerce.CatalogImport.Commands
{
    public class ImportItemDefinitionCommand : CommerceCommand
    {
        private readonly IImportItemDefinitionPipeline _pipeline;

        public ImportItemDefinitionCommand(IImportItemDefinitionPipeline pipeline, IServiceProvider serviceProvider) : base(serviceProvider)
        {
            this._pipeline = pipeline;
        }

        public async Task<ImportItemDefinitionArgument> Process(CommerceContext commerceContext, ImportItemDefinitionArgument args)
        {
            using (var activity = CommandActivity.Start(commerceContext, this))
            {
                var result = await this._pipeline.RunAsync(args, new CommercePipelineExecutionContextOptions(commerceContext));
                return result;
            }
        }
    }
}