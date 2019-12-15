//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;
//using Plugin.Sync.Commerce.EntitiesMigration.Extensions;
//using Plugin.Sync.Commerce.EntitiesMigration.Models;
//using Plugin.Sync.Commerce.EntitiesMigration.Services;
//using Serilog;
//using Sitecore.Commerce.Core;
//using Sitecore.Commerce.Core.Commands;
//using Sitecore.Commerce.Plugin.Composer;
//using Sitecore.Framework.Conditions;
//using Sitecore.Framework.Pipelines;

//namespace Plugin.Sample.GenericTaxes.Pipelines.Blocks
//{
//    /// <summary>
//    /// Import Composer Templates Block
//    /// </summary>
//    [PipelineDisplayName("SyncComposerTemplatesBlock")]
//    public class MergeComposerTemplatesBlock : PipelineBlock<ImportComposerTemplatePipelineModel, bool, CommercePipelineExecutionContext>
//    {
//        /// <summary>
//        /// Commerce Commander
//        /// </summary>
//        private readonly PersistEntityCommand _persistEntityCommand;
//        private readonly DeleteEntityCommand _deleteEntityCommand;
//        /// <summary>
//        /// Composer Template Service
//        /// </summary>
//        private readonly IEntityService _composerTemplateService;

//        /// <summary>
//        /// c'tor
//        /// </summary>
//        /// <param name="persistEntityCommand">commerceCommander</param>
//        /// <param name="composerTemplateService">Composer Template service</param>
//        public MergeComposerTemplatesBlock(
//            PersistEntityCommand persistEntityCommand,
//            IEntityService composerTemplateService,
//             DeleteEntityCommand deleteEntityCommand)
//        {
//            _persistEntityCommand = persistEntityCommand;
//            _composerTemplateService = composerTemplateService;
//            _deleteEntityCommand = deleteEntityCommand;
//        }

//        /// <summary>
//        /// Run
//        /// </summary>
//        /// <param name="arg">arg</param>
//        /// <param name="context">context</param>
//        /// <returns>flag if the process was sucessfull</returns>
//        public override async Task<bool> Run(ImportComposerTemplatePipelineModel arg, CommercePipelineExecutionContext context)
//        {
//            Condition.Requires(arg).IsNotNull($"{this.Name}: The arg can not be null");
//            Condition.Requires(arg.Arguments).IsNotNull($"{this.Name}: The import arguments can not be null");
//            Condition.Requires(arg.InputComposerTemplates).IsNotNull($"{this.Name}: The composer templates can not be null");

//            //if (arg.Arguments.ImportType != ImportType.Merge)
//            //{
//            //    return await Task.FromResult(arg);
//            //}

//            IList<ComposerTemplate> existingComposerTemplates = _composerTemplateService.GetAllEntities(context.CommerceContext);

//            foreach (CustomComposerTemplate composerTemplate in arg.InputComposerTemplates)
//            {
//                ComposerTemplate newComposerTemplate = composerTemplate.ToComposerTemplate();
//                ComposerTemplate existingComposerTemplate = existingComposerTemplates.FirstOrDefault(element => element.Id.Equals(newComposerTemplate.Id));
//                if (existingComposerTemplate != null)
//                {
//                    DeleteEntityArgument result = await _deleteEntityCommand.Process(context.CommerceContext, existingComposerTemplate.Id);
//                    if (!result.Success)
//                    {
//                        Log.Error($"OverrideComposerTemplatesBlock: Deletion of {newComposerTemplate.Id} failed - new Template was not imported");
//                    }
//                    // TODO Merge existing composer template with new one ????
//                    // If we take existing one then version and entity verison increment
//                    // If we take new one, delete existing one first
//                }

//                var persistResult = await this._persistEntityCommand.Process(context.CommerceContext, newComposerTemplate);
//            }

//            var importResult = new ImportComposerTemplatePipelineModel() { Success = true };
//            return await Task.FromResult(importResult.Success);
//        }
//    }
//}