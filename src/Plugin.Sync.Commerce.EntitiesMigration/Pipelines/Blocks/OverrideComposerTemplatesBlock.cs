//using System.Threading.Tasks;
//using Sitecore.Commerce.Core;
//using Sitecore.Framework.Conditions;
//using Sitecore.Framework.Pipelines;
//using System.Collections.Generic;
//using Sitecore.Commerce.Plugin.Composer;
//using Sitecore.Commerce.Core.Commands;
//using Plugin.Ryder.Commerce.Composer.TemplateSync.Models;
//using Plugin.Ryder.Commerce.Composer.TemplateSync.Extensions;
//using Plugin.Ryder.Commerce.Composer.TemplateSync.Pipelines.Arguments;
//using Plugin.Ryder.Commerce.Composer.TemplateSync.Services;
//using System.Linq;
//using Serilog;

//namespace Plugin.Sample.GenericTaxes.Pipelines.Blocks
//{
//    /// <summary>
//    /// Import Composer Templates Block
//    /// </summary>
//    [PipelineDisplayName("OverrideComposerTemplatesBlock")]
//    public class OverrideComposerTemplatesBlock : PipelineBlock<ImportComposerTemplatePipelineModel, ImportComposerTemplatePipelineModel, CommercePipelineExecutionContext>
//    {
//        /// <summary>
//        /// Commerce Commander
//        /// </summary>
//        private readonly PersistEntityCommand _persistEntityCommand;

//        /// <summary>
//        /// Delete Entity Command
//        /// </summary>
//        private readonly DeleteEntityCommand _deleteEntityCommand;

//        /// <summary>
//        /// Composer Template Service
//        /// </summary>
//        private readonly IComposerTemplateService _composerTemplateService;

//        /// <summary>
//        /// c'tor
//        /// </summary>
//        /// <param name="persistEntityCommand">commerceCommander</param>
//        /// <param name="composerTemplateService">Composer Template Service</param>
//        /// <param name="deleteEntityCommand">Delete Entity Command</param>
//        public OverrideComposerTemplatesBlock(
//            PersistEntityCommand persistEntityCommand,
//            IComposerTemplateService composerTemplateService,
//            DeleteEntityCommand deleteEntityCommand)
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
//        public override async Task<ImportComposerTemplatePipelineModel> Run(ImportComposerTemplatePipelineModel arg, CommercePipelineExecutionContext context)
//        {
//            Condition.Requires(arg).IsNotNull($"{this.Name}: The arg can not be null");
//            Condition.Requires(arg.Arguments).IsNotNull($"{this.Name}: The import arguments can not be null");
//            Condition.Requires(arg.InputComposerTemplates).IsNotNull($"{this.Name}: The composer templates can not be null");

//            //if (arg.Arguments.ImportType != ImportType.Override)
//            //{
//            //    return await Task.FromResult(arg);
//            //}

//            IList<ComposerTemplate> existingComposerTemplates = _composerTemplateService.GetAllComposerTemplates(context.CommerceContext);

//            foreach (CustomComposerTemplate composerTemplate in arg.InputComposerTemplates)
//            {
//                ComposerTemplate newComposerTemplate = composerTemplate.ToComposerTemplate();
//                ComposerTemplate existingComposerTemplate = existingComposerTemplates.FirstOrDefault(element => element.Id.Equals(newComposerTemplate.Id));
//                if (existingComposerTemplate != null)
//                {
//                    // Try to increase version count instead of delete
//                    DeleteEntityArgument result = await _deleteEntityCommand.Process(context.CommerceContext, existingComposerTemplate.Id);
//                    if (!result.Success)
//                    {
//                        Log.Error($"OverrideComposerTemplatesBlock: Deletion of {newComposerTemplate.Id} failed - new Template was not imported");
//                    }
//                }

//                var persistResult = await this._persistEntityCommand.Process(context.CommerceContext, newComposerTemplate);
//            }

//            arg.Success = true;
//            return await Task.FromResult(arg);
//        }
//    }
//}