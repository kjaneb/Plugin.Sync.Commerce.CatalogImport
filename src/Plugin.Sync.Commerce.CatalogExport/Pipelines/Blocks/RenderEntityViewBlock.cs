using Plugin.Sync.Commerce.CatalogExport.Pipelines.Arguments;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.Plugin.Composer;
using Sitecore.Framework.Pipelines;
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
        private readonly CommerceCommander _commerceCommander;
        private readonly ComposerCommander _composerCommander;
        #endregion

        #region Public methods
        /// <summary>
        /// Public contructor
        /// </summary>
        /// <param name="commerceCommander"></param>
        /// <param name="composerCommander"></param>
        /// <param name="importHelper"></param>
        public RenderEntityViewBlock(CommerceCommander commerceCommander, ComposerCommander composerCommander)
        {
            _commerceCommander = commerceCommander;
            _composerCommander = composerCommander;
        }

        /// <summary>
        /// Main execution point
        /// </summary>
        /// <param name="arg"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public override async Task<ExportCommerceEntityArgument> Run(ExportCommerceEntityArgument arg, CommercePipelineExecutionContext context)
        {
            await Task.CompletedTask;
            return arg;
        }
        #endregion

    }
}