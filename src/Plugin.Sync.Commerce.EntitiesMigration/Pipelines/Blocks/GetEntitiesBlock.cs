using Plugin.Sync.Commerce.EntitiesMigration.Pipelines.Arguments;
using Plugin.Sync.Commerce.EntitiesMigration.Services;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.Core.Commands;
using Sitecore.Commerce.Plugin.Composer;
using Sitecore.Framework.Conditions;
using Sitecore.Framework.Pipelines;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Sitecore.Commerce.Plugin.Catalog;

namespace Plugin.Sample.GenericTaxes.Pipelines.Blocks
{
    /// <summary>
    /// Import Composer Templates Block
    /// </summary>
    [PipelineDisplayName("GetEntitiesBlock")]
    public class GetEntitiesBlock : PipelineBlock<ExportEntitiesArgument, IList<CommerceEntity>, CommercePipelineExecutionContext>
    {
        /// <summary>
        /// Commerce Commander
        /// </summary>
        private readonly CommerceCommander _commerceCommander;

        /// <summary>
        /// Composer Template Service
        /// </summary>
        private readonly IEntityService _entityService;

        /// <summary>
        /// c'tor
        /// </summary>
        /// <param name="commerceCommander">commerceCommander</param>
        /// <param name="findEntitiesInListCommand">findEntitiesInListCommand</param>
        public GetEntitiesBlock(
            CommerceCommander commerceCommander,
            IEntityService entityService)
        {
            _commerceCommander = commerceCommander;
            _entityService = entityService;
        }

        /// <summary>
        /// Run
        /// </summary>
        /// <param name="arg">arg</param>
        /// <param name="context">context</param>
        /// <returns>flag if the process was sucessfull</returns>
        public override async Task<IList<CommerceEntity>> Run(ExportEntitiesArgument arg, CommercePipelineExecutionContext context)
        {
            Condition.Requires(arg).IsNotNull($"{this.Name}: The argument can not be null");

            switch (arg.EntityType.ToLower())
            {
                case "category":
                    return await Task.FromResult(_entityService.GetAllEntities<Category>(context.CommerceContext));
                case "sellableitem":
                    return await Task.FromResult(_entityService.GetAllEntities<SellableItem>(context.CommerceContext));
                case "composertemplate":
                    return await Task.FromResult(_entityService.GetAllEntities<ComposerTemplate>(context.CommerceContext));
                default:
                    return null;
            }
        }
    }
}