using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Plugin.Sync.Commerce.CatalogImport.Commands;
using Plugin.Sync.Commerce.CatalogImport.Extensions;
using Plugin.Sync.Commerce.CatalogImport.Pipelines.Arguments;
using Plugin.Sync.Commerce.CatalogImport.Policies;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.Core.Commands;
using Sitecore.Commerce.Plugin.Catalog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http.OData;

namespace Plugin.Sync.Commerce.CatalogImport.Controllers
{
    public class CommandsController : CommerceController
    {
        private readonly GetEnvironmentCommand _getEnvironmentCommand;
        //TODO: don't hard-code env name
        private const string ENV_NAME = "HabitatAuthoring";

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
        [DisableRequestSizeLimit]
        [Route("ImportCategory()")]
        public async Task<IActionResult> ImportCategory([FromBody] JObject request)
        {
            InitializeEnvironment();
            try
            {
                var command = Command<ImportCategoryCommand>();
                var mappingPolicy = CurrentContext.GetPolicy<CategoryMappingPolicy>();
                var argument = new ImportCatalogEntityArgument(request, mappingPolicy, typeof(Category));
                var result = await command.Process(CurrentContext, argument);

                return result != null ? new ObjectResult(result) : new NotFoundObjectResult("Error importing Category data");
            }
            catch (Exception ex)
            {
                return new ObjectResult(ex);
            }

        }

        
        /// <summary>
        /// Sync incoming data into Commerce SellableItem
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [DisableRequestSizeLimit]
        [Route("ImportSellableItem()")]
        public async Task<IActionResult> ImportSellableItem([FromBody] JObject request)
        {
            InitializeEnvironment();
            try
            {
                var command = Command<ImportSellableItemCommand>();
                var mappingPolicy = CurrentContext.GetPolicy<SellableItemMappingPolicy>();
                var argument = new ImportCatalogEntityArgument(request, mappingPolicy, typeof(SellableItem));
                var result = await command.Process(CurrentContext, argument);

                return result != null ? new ObjectResult(result) : new NotFoundObjectResult("Error importing SellableItem data");
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