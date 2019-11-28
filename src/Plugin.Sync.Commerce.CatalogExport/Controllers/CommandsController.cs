using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Plugin.Sync.Commerce.CatalogExport.Pipelines.Arguments;
using Plugin.Sync.Commerce.CatalogImport.Commands;
//using Plugin.Sync.Commerce.CatalogExport.Commands;
//using Plugin.Sync.Commerce.CatalogExport.Extensions;
//using Plugin.Sync.Commerce.CatalogExport.Pipelines.Arguments;
//using Plugin.Sync.Commerce.CatalogExport.Policies;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.Core.Commands;
using Sitecore.Commerce.Plugin.Catalog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http.OData;

namespace Plugin.Sync.Commerce.CatalogExport.Controllers
{
    public class CommandsController : CommerceController
    {
        private readonly GetEnvironmentCommand _getEnvironmentCommand;
        //TODO: don't hard-code env name
        //private const string ENV_NAME = "HabitatAuthoring";

        public CommandsController(IServiceProvider serviceProvider, CommerceEnvironment globalEnvironment, GetEnvironmentCommand getEnvironmentCommand) : base(serviceProvider, globalEnvironment)
        {
            _getEnvironmentCommand = getEnvironmentCommand;
        }

        /// <summary>
        /// Import Category data
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("ExportCommerceEntity()")]
        public async Task<IActionResult> ExportCommerceEntity([FromBody] JObject request)
        {
            InitializeEnvironment();
            try
            {
                var command = Command<ExportCommerceEntityCommand>();
                //var mappingPolicy = CurrentContext.GetPolicy<CategoryMappingPolicy>();
                var entityId = request.GetValue("entityId").Value<string>();
                var view = request.GetValue("view").Value<string>();
                var argument = new ExportCommerceEntityArgument(entityId, view);
                var result = await command.Process(CurrentContext, argument);

                if (result == null || !result.Success)
                {
                    if (result != null && result.EntityNotFound)
                        return new NotFoundObjectResult(result.ErrorMessage);

                    else
                        return new UnprocessableEntityObjectResult($"Error rendering view for Entity ID={entityId}. {CurrentContext.PipelineContext.AbortReason}");
                }

                return new ObjectResult(result.Response);
            }
            catch (Exception ex)
            {
                return new ObjectResult(ex);
            }

        }

        /// <summary>
        /// Set default environment
        /// </summary>
        /// <returns></returns>
        private void InitializeEnvironment()
        {
            var commerceEnvironment = this.CurrentContext.Environment;
                //await _getEnvironmentCommand.Process(this.CurrentContext, ENV_NAME) ??
            var pipelineContextOptions = this.CurrentContext.PipelineContextOptions;
            pipelineContextOptions.CommerceContext.Environment = commerceEnvironment;
            this.CurrentContext.PipelineContextOptions.CommerceContext.Environment = commerceEnvironment;
        }
    }
}