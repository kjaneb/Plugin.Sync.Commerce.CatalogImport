//using System.Threading.Tasks;
//using Sitecore.Commerce.Core;
//using Sitecore.Framework.Conditions;
//using Sitecore.Framework.Pipelines;
//using System.Collections.Generic;
//using Sitecore.Commerce.Plugin.Composer;
//using Sitecore.Commerce.Core.Commands;
//using Plugin.Sync.Commerce.EntitiesMigration.Models;
//using Plugin.Sync.Commerce.EntitiesMigration.Extensions;
//using Plugin.Sync.Commerce.EntitiesMigration.Pipelines.Arguments;
//using Plugin.Sync.Commerce.EntitiesMigration.Services;
//using System.Linq;
//using Serilog;

//namespace Plugin.Sample.GenericTaxes.Pipelines.Blocks
//{
//    /// <summary>
//    /// Import Composer Templates Block
//    /// </summary>
//    [PipelineDisplayName("SkipComposerTemplatesBlock")]
//    public class SkipComposerTemplatesBlock : PipelineBlock<ImportComposerTemplatePipelineModel, ImportComposerTemplatePipelineModel, CommercePipelineExecutionContext>
//    {
//        /// <summary>
//        /// Commerce Commander
//        /// </summary>
//        private readonly PersistEntityCommand _persistEntityCommand;

//        /// <summary>
//        /// Composer Template Service
//        /// </summary>
//        private readonly IEntityService _composerTemplateService;

//        /// <summary>
//        /// c'tor
//        /// </summary>
//        /// <param name="persistEntityCommand">commerceCommander</param>
//        /// <param name="composerTemplateService">Composer Template service</param>
//        public SkipComposerTemplatesBlock(
//            PersistEntityCommand persistEntityCommand,
//            IEntityService composerTemplateService)
//        {
//            _persistEntityCommand = persistEntityCommand;
//            _composerTemplateService = composerTemplateService;
//        }

//        /// <summary>
//        /// Run
//        /// </summary>
//        /// <param name="arg">arg</param>
//        /// <param name="context">context</param>
//        /// <returns>flag if the process was sucessfull</returns>
//        public override async Task<ImportComposerTemplatePipelineModel> Run(ImportComposerTemplatePipelineModel arg, CommercePipelineExecutionContext context)
//        {
//            Condition.Requires(arg).IsNotNull($"{this.Name}: The arg can not be null");
//            Condition.Requires(arg.Arguments).IsNotNull($"{this.Name}: The import arguments can not be null");
//            Condition.Requires(arg.InputComposerTemplates).IsNotNull($"{this.Name}: The composer templates can not be null");

//            if (arg.Arguments.ImportType != ImportType.Skip)
//            {
//                return await Task.FromResult(arg);
//            }

//            IList<ComposerTemplate> existingComposerTemplates = _composerTemplateService.GetAllEntities(context.CommerceContext);

//            foreach (CustomComposerTemplate composerTemplate in arg.InputComposerTemplates)
//            {
//                ComposerTemplate newComposerTemplate = composerTemplate.ToComposerTemplate();
//                ComposerTemplate existingComposerTemplate = existingComposerTemplates.FirstOrDefault(element => element.Id.Equals(newComposerTemplate.Id));
//                if (existingComposerTemplate != null)
//                {
//                    Log.Information($"SkipComposerTemplatesBlock: Skipping import of {newComposerTemplate.Id}");
//                    continue;
//                }

//                var persistResult = await this ._persistEntityCommand.Process(context.CommerceContext, newComposerTemplate);
//            }

//            arg.Success = true;
//            return await Task.FromResult(arg);
//        }
//    }
//}