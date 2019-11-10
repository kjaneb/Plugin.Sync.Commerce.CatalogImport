using Plugin.Sync.Commerce.CatalogImport.Pipelines;
using Plugin.Sync.Commerce.CatalogImport.Pipelines.Arguments;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.Core.Commands;
using System;
using System.Threading.Tasks;

namespace Plugin.Sync.Commerce.CatalogImport.Commands
{
    /// <summary>
    /// Import Product Record
    /// </summary>
    public class ProductImportCommand : CommerceCommand
    {
        private readonly IProductImportMinionPipeline _pipeline;

        public ProductImportCommand(IProductImportMinionPipeline pipeline, IServiceProvider serviceProvider) : base(serviceProvider)
        {
            this._pipeline = pipeline;
        }

        public async Task<MinionRunResultsModel> Process(CommerceContext commerceContext, ProductImportMinionArgument args)
        {
            using (var activity = CommandActivity.Start(commerceContext, this))
            {
                var result = await this._pipeline.Run(args, new CommercePipelineExecutionContextOptions(commerceContext));
                return result;
            }
        }
    }
}