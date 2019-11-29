using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Plugin.Ryder.Commerce.CatalogExport.Util;
using Plugin.Sync.Commerce.CatalogExport.Models;
using Plugin.Sync.Commerce.CatalogExport.Pipelines.Arguments;
using RazorLight;
using RazorLight.Razor;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.Plugin.Composer;
using Sitecore.Framework.Pipelines;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Plugin.Sync.Commerce.CatalogExport.Pipelines.Blocks
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
        //IServiceProvider _serviceProvider;
        //RazorLightEngine _razorEngine;
        IHostingEnvironment _hostingEnvironment;
        #endregion

        #region Public methods
        /// <summary>
        /// Public contructor
        /// </summary>
        /// <param name="commerceCommander"></param>
        /// <param name="composerCommander"></param>
        /// <param name="importHelper"></param>
        //public RenderEntityViewBlock(CommerceCommander commerceCommander, ComposerCommander composerCommander)
        public RenderEntityViewBlock(IHostingEnvironment hostingEnvironment /*IServiceProvider serviceProvider/*IViewRenderService viewRenderService*/)
        {
            _hostingEnvironment = hostingEnvironment;
            //_serviceProvider = serviceProvider;
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
            var engine = new RazorLightEngineBuilder()
              .UseFileSystemProject(_hostingEnvironment.WebRootPath)
              .UseMemoryCachingProvider()
              .Build();

            var model = context.GetModel<EntityDataModel>();
            if (model == null)
            {
                context.AbortPipeline(arg, $"EntityDataModel must be initialied and added to CommercePipelineExecutionContext prior to calling {this.GetType().Name}. Entity ID={arg.EntityId} not found.");
            }
            if (arg.ViewTemplate == null)
            {
                context.AbortPipeline(arg, $"ViewTemplate must be initialied {this.GetType().Name}. Entity ID={arg.EntityId}.");
            }

            arg.Response = await engine.CompileRenderStringAsync(arg.ViewTemplate, "Hello World @Model.Name", model.Entity);
            return arg;
        }
        #endregion

    }
}