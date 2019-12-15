using System.Threading.Tasks;
using Sitecore.Commerce.Core;
using Sitecore.Framework.Conditions;
using Sitecore.Framework.Pipelines;
using System.Collections.Generic;
using Plugin.Sync.Commerce.EntitiesMigration.Pipelines.Arguments;
using Newtonsoft.Json;
using System.IO;
using Plugin.Sync.Commerce.EntitiesMigration.Policies;
using Plugin.Sync.Commerce.EntitiesMigration.Models;

namespace Plugin.Sample.GenericTaxes.Pipelines.Blocks
{
    /// <summary>
    /// Import Composer Templates Block
    /// </summary>
    [PipelineDisplayName("ReadComposerTemplatesFromDisc")]
    public class ReadComposerTemplates : PipelineBlock<ImportEntitiesArgument, ImportComposerTemplatePipelineModel, CommercePipelineExecutionContext>
    {
        /// <summary>
        /// Run
        /// </summary>
        /// <param name="arg">arg</param>
        /// <param name="context">context</param>
        /// <returns>flag if the process was sucessfull</returns>
        public override async Task<ImportComposerTemplatePipelineModel> Run(ImportEntitiesArgument arg, CommercePipelineExecutionContext context)
        {
            Condition.Requires(arg).IsNotNull($"{this.Name}: The argument can not be null");
            Condition.Requires(arg.InputJson).IsNotNullOrEmpty($"{this.Name}: InputJson can not be null");
            //ComposerTemplatesSyncPolicy policy = context.GetPolicy<ComposerTemplatesSyncPolicy>();

            //string inputJson = File.ReadAllText(policy.PathToJson);

            List<CustomComposerTemplate> allComposerTemplates = JsonConvert.DeserializeObject<List<CustomComposerTemplate>>(arg.InputJson);

            return await Task.FromResult(new ImportComposerTemplatePipelineModel()
            {
                Arguments = arg,
                InputComposerTemplates = allComposerTemplates
            });
        }
    }
}