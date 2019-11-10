using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Plugin.Sync.Commerce.CatalogImport.Entities;
using Plugin.Sync.Commerce.CatalogImport.Pipelines.Arguments;
using Plugin.Sync.Commerce.CatalogImport.Policies;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.Plugin.Catalog;
using Sitecore.Framework.Pipelines;

namespace Plugin.Sync.Commerce.CatalogImport.Pipelines.Blocks
{
    [PipelineDisplayName("AddSellableItemToUpdatedSellableItemsListBlock")]
    public class AddSellableItemToUpdatedSellableItemsListBlock : PipelineBlock<PersistEntityArgument, PersistEntityArgument, CommercePipelineExecutionContext>
    {
        private readonly IAddListEntitiesPipeline _addListEntitiesPipeline;
        public AddSellableItemToUpdatedSellableItemsListBlock(IAddListEntitiesPipeline addListEntitiesPipeline)
        {
            _addListEntitiesPipeline = addListEntitiesPipeline;
        }
        public override async Task<PersistEntityArgument> Run(PersistEntityArgument arg, CommercePipelineExecutionContext context)
        {
            
            // Only do something if the Entity is an sellableitem 
            if (!(arg.Entity is SellableItem))
            {
                return arg;
            }

            var policy = context.CommerceContext.GetPolicy<SellableItemMappingPolicy>();

            ListEntitiesArgument listArgument = new ListEntitiesArgument(new string[1] { arg.Entity.Id }, policy.UpdatedItemsList);

            ListEntitiesArgument addToListResult = await this._addListEntitiesPipeline.Run(listArgument, context);

            //context.Logger.LogInformation($"{this.Name} - Run AddSellableItemToUpdatedSellableItemsListBlock:{arg.Entity.Id}");
            return arg;
        }
    }
}