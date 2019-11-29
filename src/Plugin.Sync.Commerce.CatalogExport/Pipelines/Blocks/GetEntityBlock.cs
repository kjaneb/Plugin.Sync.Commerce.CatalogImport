using Microsoft.Extensions.DependencyInjection;
using Plugin.Sync.Commerce.CatalogExport.Models;
using Plugin.Sync.Commerce.CatalogExport.Pipelines.Arguments;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.Core.Commands;
using Sitecore.Commerce.Plugin.Composer;
using Sitecore.Framework.Pipelines;
using System;
using System.Threading.Tasks;

namespace PPlugin.Sync.Commerce.CatalogExport.Pipelines.Blocks
{
    /// <summary>
    /// Import data into an existing SellableItem or new SellableItem entity
    /// </summary>
    [PipelineDisplayName("GetEntityBlock")]
    public class GetEntityBlock : PipelineBlock<ExportCommerceEntityArgument, ExportCommerceEntityArgument, CommercePipelineExecutionContext>
    {
        #region Private fields
        private readonly CommerceCommander _commerceCommander;
        #endregion

        #region Public methods
        
        public GetEntityBlock(CommerceCommander commerceCommander)
        {
            _commerceCommander = commerceCommander;
        }

        /// <summary>
        /// Main execution point
        /// </summary>
        /// <param name="arg"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public override async Task<ExportCommerceEntityArgument> Run(ExportCommerceEntityArgument arg, CommercePipelineExecutionContext context)
        {
            var entity = await _commerceCommander.Command<FindEntityCommand>().Process(context.CommerceContext, typeof(CommerceEntity), arg.EntityId);
            if (entity == null)
            {
                var errorMessage = $"CommerceEntity with ID={arg.EntityId} not found.";
                arg.Success = false;
                arg.EntityNotFound = true;
                arg.ErrorMessage = errorMessage;
                context.Abort(errorMessage, arg);
            }

            context.AddModel(new EntityDataModel(entity));
            return arg;
        }
        #endregion

    }
}