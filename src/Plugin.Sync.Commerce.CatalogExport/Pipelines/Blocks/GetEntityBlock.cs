using Plugin.Sync.Commerce.CatalogExport.Models;
using Plugin.Sync.Commerce.CatalogExport.Pipelines.Arguments;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.Core.Commands;
using Sitecore.Framework.Pipelines;
using System.Threading.Tasks;

namespace Plugin.Sync.Commerce.CatalogExport.Pipelines.Blocks
{
    /// <summary>
    /// Get entity object from Commerce
    /// </summary>
    [PipelineDisplayName("GetEntityBlock")]
    public class GetEntityBlock : PipelineBlock<ExportCommerceEntityArgument, ExportCommerceEntityArgument, CommercePipelineExecutionContext>
    {
        #region Private fields
        private readonly CommerceCommander _commerceCommander;
        #endregion

        #region Public methods
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="commerceCommander"></param>
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