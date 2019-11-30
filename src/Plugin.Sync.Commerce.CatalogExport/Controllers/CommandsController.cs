using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Plugin.Ryder.Commerce.CatalogExport.Util;
using Plugin.Sync.Commerce.CatalogExport.Pipelines.Arguments;
using Plugin.Sync.Commerce.CatalogImport.Commands;
//using Plugin.Sync.Commerce.CatalogExport.Commands;
//using Plugin.Sync.Commerce.CatalogExport.Extensions;
//using Plugin.Sync.Commerce.CatalogExport.Pipelines.Arguments;
//using Plugin.Sync.Commerce.CatalogExport.Policies;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.Core.Commands;
using Sitecore.Commerce.Plugin.Catalog;
using Sitecore.Framework.Conditions;
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
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <param name="globalEnvironment"></param>
        /// <param name="getEnvironmentCommand"></param>
        public CommandsController(IServiceProvider serviceProvider, CommerceEnvironment globalEnvironment, GetEnvironmentCommand getEnvironmentCommand) : base(serviceProvider, globalEnvironment)
        {
            _getEnvironmentCommand = getEnvironmentCommand;
        }

        /// <summary>
        /// Render view using Razor template and Commerce entity object as its model
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [DisableRequestSizeLimit]
        [Route("RenderEntityView()")]
        public async Task<IActionResult> RenderEntityView([FromBody] JObject request)
        {
            InitializeEnvironment();
            try
            {
                var command = Command<ExportCommerceEntityCommand>();

                var entityId = request.SelectValue<string>("entityId");
                var templatePath = request.SelectValue<string>("templatePath");
                var templateLocation = request.SelectValue<string>("templateLocation");

                Condition.Requires(entityId, "entityId parameter is required").IsNotNullOrEmpty();
                Condition.Requires(templatePath, "templatePath parameter is required").IsNotNullOrEmpty();
                Condition.Requires(templateLocation, "templateLocation parameter is required").IsNotNullOrEmpty();

                var argument = new ExportCommerceEntityArgument(entityId, templatePath, templateLocation);
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
        /// Render view using Razor template and Commerce entity object as its model
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [DisableRequestSizeLimit]
        [Route("RenderEntityView()")]
        public async Task<IActionResult> RenderEntityViews([FromBody] JObject request)
        {
            InitializeEnvironment();
            try
            {
                var command = Command<ExportCommerceEntityCommand>();

                var entityIds = request.SelectValue<string>("entityIds");
                var templatePath = request.SelectValue<string>("templatePath");
                var templateLocation = request.SelectValue<string>("templateLocation");
                var breakOnErrorString = request.SelectValue<string>("breakonerror");

                Condition.Requires(entityIds, "entityIds parameter is required").IsNotNullOrEmpty();
                Condition.Requires(templatePath, "templatePath parameter is required").IsNotNullOrEmpty();
                Condition.Requires(templateLocation, "templateLocation parameter is required").IsNotNullOrEmpty();

                var breakOnError = false;
                if (!string.IsNullOrEmpty(breakOnErrorString))
                    bool.TryParse(breakOnErrorString, out breakOnError);


                var entityIDsAarray = entityIds.Split('|');
                var responses = new List<string>();
                foreach (var entityId in entityIDsAarray)
                {
                    var argument = new ExportCommerceEntityArgument(entityId, templatePath, templateLocation);
                    var result = await command.Process(CurrentContext, argument);

                    if (result != null && result.Success)
                    {
                        responses.Add(result.Response);
                    }
                    else if (breakOnError)
                    {
                        if (result != null && result.EntityNotFound)
                            return new NotFoundObjectResult(result.ErrorMessage);

                        else
                            return new UnprocessableEntityObjectResult($"Error rendering view for Entity ID={entityIds}. {CurrentContext.PipelineContext.AbortReason}");
                    }
                }

                return new ObjectResult(responses);
            }
            catch (Exception ex)
            {
                return new ObjectResult(ex);
            }
        }

        /// <summary>
        /// Init context pipeline
        /// </summary>
        /// <returns></returns>
        private void InitializeEnvironment()
        {
            var commerceEnvironment = this.CurrentContext.Environment;
            var pipelineContextOptions = this.CurrentContext.PipelineContextOptions;
            pipelineContextOptions.CommerceContext.Environment = commerceEnvironment;
            this.CurrentContext.PipelineContextOptions.CommerceContext.Environment = commerceEnvironment;
        }
    }
}