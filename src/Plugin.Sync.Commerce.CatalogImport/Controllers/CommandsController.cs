using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Plugin.Sync.Commerce.CatalogImport.Commands;
using Plugin.Sync.Commerce.CatalogImport.Pipelines.Arguments;
using Plugin.Sync.Commerce.CatalogImport.Policies;
using Serilog;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.Core.Commands;
using Sitecore.Commerce.Plugin.Catalog;
using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Routing;
using Plugin.Sync.Commerce.CatalogImport.Models;

namespace Plugin.Sync.Commerce.CatalogImport.Controllers
{
    /// <summary>
    /// Catalog Entities Import Controller
    /// </summary>
    public class CommandsController : CommerceODataController
    {
        private readonly GetEnvironmentCommand _getEnvironmentCommand;

        //TODO: move below consts into CH connection policy
        static string _connectionString = "Endpoint=sb://xccontenthubdemo.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=HxyFUilEe7vGB0gXAYPnBUVQA8YaG63ElEJPkaJ5Pe4=";
        static string _subscriptionName = "sitecore";
        static string _topicName = "products_content";
        static int _maxMessagesCount = 100;

        /// <summary>
        /// Public constructor with DI
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <param name="globalEnvironment"></param>
        /// <param name="getEnvironmentCommand"></param>
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
        [ODataRoute("ImportCategory()", RouteName = "api")]
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
        [ODataRoute("ImportSellableItem()", RouteName = "api")]
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

        [HttpPost]
        [DisableRequestSizeLimit]
        [ODataRoute("ImportVariant()", RouteName = "api")]
        public async Task<IActionResult> ImportVariant([FromBody] JObject request)
        {
            InitializeEnvironment();
            try
            {
                var command = Command<ImportVariantCommand>();
                var mappingPolicy = CurrentContext.GetPolicy<VariantMappingPolicy>();
                var argument = new ImportCatalogEntityArgument(request, mappingPolicy, typeof(Sitecore.Commerce.Plugin.Catalog.SellableItem));
                var result = await command.Process(CurrentContext, argument);

                return result != null ? new ObjectResult(result) : new NotFoundObjectResult("Error importing Variant data");
            }
            catch (Exception ex)
            {
                return new ObjectResult(ex);
            }
        }

        [HttpPost]
        [ODataRoute("ImportItemDefinition", RouteName = "api")]
        public async Task<IActionResult> ImportItemDefinition([FromBody] ODataActionParameters value)
        {
            InitializeEnvironment();
            try
            {
                var command = Command<ImportItemDefinitionCommand>();

                var argument = new ImportItemDefinitionArgument();
                argument.Name = value["name"].ToString();
                argument.DisplayName = value["displayName"].ToString();
                var fields = value["properties"] as ItemDefinitionProperties;
                argument.Fields = fields.Values;

                var result = await command.Process(CurrentContext, argument);

                return result != null ? new ObjectResult(result) : new NotFoundObjectResult("Error importing Variant data");
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