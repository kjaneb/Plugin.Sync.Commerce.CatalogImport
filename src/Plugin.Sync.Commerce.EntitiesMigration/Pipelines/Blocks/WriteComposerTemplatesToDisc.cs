using System;
using System.Threading.Tasks;
using Sitecore.Commerce.Core;
using Sitecore.Framework.Conditions;
using Sitecore.Framework.Pipelines;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.IO;
using Serilog;
using Plugin.Sync.Commerce.EntitiesMigration.Policies;
using Plugin.Sync.Commerce.EntitiesMigration.Models;

namespace Plugin.Sample.GenericTaxes.Pipelines.Blocks
{
    /// <summary>
    /// Import Composer Templates Block
    /// </summary>
    [PipelineDisplayName("WriteComposerTemplatesToDisc")]
    public class WriteComposerTemplatesToDisc : PipelineBlock<IList<CustomComposerTemplate>, string, CommercePipelineExecutionContext>
    {
        /// <summary>
        /// Commerce Commander
        /// </summary>
        private readonly CommerceCommander _commerceCommander;

        /// <summary>
        /// c'tor
        /// </summary>
        /// <param name="commerceCommander">commerceCommander</param>
        public WriteComposerTemplatesToDisc(
            CommerceCommander commerceCommander)
        {
            _commerceCommander = commerceCommander;
        }

        /// <summary>
        /// Run
        /// </summary>
        /// <param name="arg">arg</param>
        /// <param name="context">context</param>
        /// <returns>flag if the process was sucessfull</returns>
        public override async Task<string> Run(IList<CustomComposerTemplate> arg, CommercePipelineExecutionContext context)
        {
            Condition.Requires(arg).IsNotNull($"{this.Name}: The argument can not be null");
            ComposerTemplatesSyncPolicy policy = context.GetPolicy<ComposerTemplatesSyncPolicy>();

            try
            {
                return JsonConvert.SerializeObject(arg);
                //string json = JsonConvert.SerializeObject(arg);
                //using (var writer = File.CreateText(policy.PathToJson))
                //{
                //    writer.Write(json);
                //}
            }
            catch (Exception e)
            {
                Log.Information("WriteEntityViewsToDisc failed to write to disc ... " + e.Message);
                return await Task.FromResult(e.Message);
            }
       
            //return await Task.FromResult(true);
        } 
    }
}