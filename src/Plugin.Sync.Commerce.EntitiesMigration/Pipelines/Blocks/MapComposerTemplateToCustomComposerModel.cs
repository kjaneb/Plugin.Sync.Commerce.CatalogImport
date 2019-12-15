using System;
using System.Threading.Tasks;
using Sitecore.Commerce.Core;
using Sitecore.Framework.Conditions;
using Sitecore.Framework.Pipelines;
using System.Collections.Generic;
using Sitecore.Commerce.Plugin.Composer;
using Sitecore.Commerce.Core.Commands;
using Plugin.Sync.Commerce.EntitiesMigration.Models;
using Plugin.Sync.Commerce.EntitiesMigration.Extensions;

namespace Plugin.Sample.GenericTaxes.Pipelines.Blocks
{
    /// <summary>
    /// Import Composer Templates Block
    /// </summary>
    [PipelineDisplayName("MapComposerTemplateToCustomComposerModel")]
    public class MapComposerTemplateToCustomComposerModel : PipelineBlock<IList<ComposerTemplate>, IList<CustomComposerTemplate>, CommercePipelineExecutionContext>
    {
        /// <summary>
        /// Commerce Commander
        /// </summary>
        private readonly CommerceCommander _commerceCommander;

        /// <summary>
        /// Find Entities In List Command
        /// </summary>
        private readonly FindEntitiesInListCommand _findEntitiesInListCommand;

        /// <summary>
        /// c'tor
        /// </summary>
        /// <param name="commerceCommander">commerceCommander</param>
        /// <param name="findEntitiesInListCommand">findEntitiesInListCommand</param>
        public MapComposerTemplateToCustomComposerModel(
            CommerceCommander commerceCommander,
            FindEntitiesInListCommand findEntitiesInListCommand)
        {
            _commerceCommander = commerceCommander;
            _findEntitiesInListCommand = findEntitiesInListCommand;
        }

        /// <summary>
        /// Run
        /// </summary>
        /// <param name="arg">arg</param>
        /// <param name="context">context</param>
        /// <returns>flag if the process was sucessfull</returns>
        public override async Task<IList<CustomComposerTemplate>> Run(IList<ComposerTemplate> arg, CommercePipelineExecutionContext context)
        {
            Condition.Requires(arg).IsNotNull($"{this.Name}: The argument can not be null");

            IList<CustomComposerTemplate> customComposerTemplates = new List<CustomComposerTemplate>();

            //foreach (ComposerTemplate composerTemplate in arg)
            //{
            //    customComposerTemplates.Add(composerTemplate.ToCustomComposerTemplate());
            //}

            return await Task.FromResult(customComposerTemplates);
        }
    }
}