using Plugin.Sync.Commerce.CatalogImport.Policies;
using Sitecore.Commerce.Core;
using Sitecore.Framework.Pipelines;
using System.Threading.Tasks;

namespace Plugin.Sync.Commerce.CatalogImport.Pipelines.Blocks
{
    [PipelineDisplayName("AddSellableItemToUpdatedSellableItemsListBlock")]
    public class AddSellableItemToUpdatedSellableItemsListBlock : AsyncPipelineBlock<PersistEntityArgument, PersistEntityArgument, CommercePipelineExecutionContext>
    {
        private readonly IAddListEntitiesPipeline _addListEntitiesPipeline;
        public AddSellableItemToUpdatedSellableItemsListBlock(IAddListEntitiesPipeline addListEntitiesPipeline)
        {
            _addListEntitiesPipeline = addListEntitiesPipeline;
        }
        public override async Task<PersistEntityArgument> RunAsync(PersistEntityArgument arg, CommercePipelineExecutionContext context)
        {
            var policy = context.CommerceContext.GetPolicy<MappingPolicyBase>();
            ListEntitiesArgument listArgument = new ListEntitiesArgument(new string[1] { arg.Entity.Id }, policy.UpdatedItemsList);
            ListEntitiesArgument addToListResult = await this._addListEntitiesPipeline.RunAsync(listArgument, context);
            return arg;
        }
    }
}