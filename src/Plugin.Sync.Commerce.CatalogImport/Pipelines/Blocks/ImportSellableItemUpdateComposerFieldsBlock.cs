using Plugin.Sync.Commerce.CatalogImport.Extensions;
using Plugin.Sync.Commerce.CatalogImport.Pipelines.Arguments;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.Plugin.Catalog;
using Sitecore.Commerce.Plugin.Composer;
using Sitecore.Framework.Pipelines;
using System.Threading.Tasks;

namespace Plugin.Sync.Commerce.CatalogImport.Pipelines.Blocks
{
    /// <summary>
    /// Import data into an existing SellableItem or new SellableItem entity
    /// </summary>
    [PipelineDisplayName("ImportSellableItemUpdateComposerFieldsBlock")]
    public class ImportSellableItemUpdateComposerFieldsBlock : PipelineBlock<ImportSellableItemArgument, ImportSellableItemArgument, CommercePipelineExecutionContext>
    {
        #region Private fields
        private readonly CommerceCommander _commerceCommander;
        private readonly ComposerCommander _composerCommander;
        private readonly CommerceEntityImportHelper _importHelper;
        #endregion

        #region Public methods
        /// <summary>
        /// Public contructor
        /// </summary>
        /// <param name="commerceCommander"></param>
        /// <param name="composerCommander"></param>
        /// <param name="importHelper"></param>
        public ImportSellableItemUpdateComposerFieldsBlock(CommerceCommander commerceCommander, ComposerCommander composerCommander)
        {
            _commerceCommander = commerceCommander;
            _composerCommander = composerCommander;
            _importHelper = new CommerceEntityImportHelper(commerceCommander, composerCommander);
        }

        /// <summary>
        /// Main execution point
        /// </summary>
        /// <param name="arg"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public override async Task<ImportSellableItemArgument> Run(ImportSellableItemArgument arg, CommercePipelineExecutionContext context)
        {
            arg.SellableItem = await _importHelper.ImportComposerViewsFields(arg.SellableItem, arg.EntityData.ComposerFields, context.CommerceContext) as SellableItem;
            return arg;
        }
        #endregion
       
    }
}