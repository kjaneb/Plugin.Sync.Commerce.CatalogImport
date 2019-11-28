using Microsoft.Extensions.DependencyInjection;
using Plugin.Sync.Commerce.CatalogExport.Models;
using Plugin.Sync.Commerce.CatalogExport.Pipelines.Arguments;
using Plugin.Sync.Commerce.CatalogExport.Services;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.Plugin.Composer;
using Sitecore.Framework.Pipelines;
using System;
using System.Threading.Tasks;

namespace PPlugin.Sync.Commerce.CatalogExport.Pipelines.Blocks
{
    /// <summary>
    /// Import data into an existing SellableItem or new SellableItem entity
    /// </summary>
    [PipelineDisplayName("RenderEntityViewBlock")]
    public class RenderEntityViewBlock : PipelineBlock<ExportCommerceEntityArgument, ExportCommerceEntityArgument, CommercePipelineExecutionContext>
    {
        #region Private fields
        //private readonly CommerceCommander _commerceCommander;
        //private readonly ComposerCommander _composerCommander;
        //private readonly IViewRenderService _viewRenderService;
        IServiceProvider _serviceProvider;
        #endregion

        #region Public methods
        /// <summary>
        /// Public contructor
        /// </summary>
        /// <param name="commerceCommander"></param>
        /// <param name="composerCommander"></param>
        /// <param name="importHelper"></param>
        //public RenderEntityViewBlock(CommerceCommander commerceCommander, ComposerCommander composerCommander)
        public RenderEntityViewBlock(IServiceProvider serviceProvider/*IViewRenderService viewRenderService*/)
        {
            _serviceProvider = serviceProvider;
            //_viewRenderService = viewRenderService;
            //_commerceCommander = commerceCommander;
            //_composerCommander = composerCommander;
        }

        /// <summary>
        /// Main execution point
        /// </summary>
        /// <param name="arg"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public override async Task<ExportCommerceEntityArgument> Run(ExportCommerceEntityArgument arg, CommercePipelineExecutionContext context)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                IViewRenderService viewRenderService = scope.ServiceProvider.GetRequiredService<IViewRenderService>();
                var model = context.GetModel<EntityDataModel>();
                if (model == null)
                {
                    var errorMessage = $"EntityDataModel must be initialied and added to CommercePipelineExecutionContext prior to calling {this.GetType().Name}. Entity ID={arg.EntityId} not found.";
                    arg.Success = false;
                    arg.ErrorMessage = errorMessage;
                    context.Abort(errorMessage, arg);
                }
                arg.Response = await viewRenderService.RenderAsync(arg.View, model.Entity);
                await Task.CompletedTask;
                return arg;
            }
        }
        #endregion

    }
}