using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Plugin.Sync.Commerce.EntitiesMigration.Models;
using Plugin.Sync.Commerce.EntitiesMigration.Pipelines;
using Plugin.Sync.Commerce.EntitiesMigration.Pipelines.Arguments;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.Core.Commands;
using Sitecore.Commerce.Plugin.Composer;

namespace Plugin.Sync.Commerce.EntitiesMigration.Commands
{
    /// <summary>
    /// ImportComposerTemplatesCommand
    /// </summary>
    public class ExportCommerceEntitiesCommand : CommerceCommand
    {
        /// <summary>
        /// Import Composer Template Pipeline
        /// </summary>
        private readonly IExportEntitiesPipeline _pipeline;

        /// <summary>
        /// c'tor
        /// </summary>
        /// <param name="pipeline">Import Composer Template Pipeline</param>
        /// <param name="serviceProvider">Service Provider</param>
        public ExportCommerceEntitiesCommand(IExportEntitiesPipeline pipeline, IServiceProvider serviceProvider) : base(serviceProvider)
        {
            this._pipeline = pipeline;
        }

        /// <summary>
        /// Process
        /// </summary>
        /// <param name="commerceContext">commerceContext</param>
        /// <returns>true if the process was successful</returns>
        public async Task<IList<CommerceEntity>> Process(CommerceContext commerceContext, ExportEntitiesArgument arg)
        {
            using (var activity = CommandActivity.Start(commerceContext, this))
            {
                return await this._pipeline.Run(arg, new CommercePipelineExecutionContextOptions(commerceContext));
                //return result;
            }
        }
    }
}
