using Microsoft.AspNetCore.Mvc;
using Microsoft.ServiceBus.Messaging;
using Newtonsoft.Json.Linq;
using Plugin.Sync.Commerce.CatalogImport.Commands;
using Plugin.Sync.Commerce.CatalogImport.Extensions;
using Plugin.Sync.Commerce.CatalogImport.Pipelines.Arguments;
using Plugin.Sync.Commerce.CatalogImport.Policies;
using Serilog;
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

        static string _connectionString = "Endpoint=sb://xccontenthubdemo.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=HxyFUilEe7vGB0gXAYPnBUVQA8YaG63ElEJPkaJ5Pe4=";
        static string _subscriptionName = "sitecore";
        static string _topicName = "products_content";
        static int _maxMessagesCount = 100;

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

        [HttpPost]
        [Route("ImportSellableItemFromContentHub()")]
        public async Task<IActionResult> ImportSellableItemFromContentHub([FromBody] JObject request)
        {
            InitializeEnvironment();
            try
            {
                if (!request.ContainsKey("EntityId") || request["EntityId"] == null)
                    return (IActionResult)new BadRequestObjectResult((object)request);
                string entityId = request["EntityId"].ToString();

                var command = Command<ImportSellableItemFromContentHubCommand>();
                var mappingPolicy = CurrentContext.GetPolicy<SellableItemMappingPolicy>();
                var argument = new ImportCatalogEntityArgument(mappingPolicy, typeof(SellableItem))
                {
                    ContentHubEntityId = entityId
                };
                var result = await command.Process(CurrentContext, argument);

                return result != null ? new ObjectResult(result) : new NotFoundObjectResult("Error importing SellableItem data");
            }
            catch (Exception ex)
            {
                return new ObjectResult(ex);
            }
        }

        [HttpPost]
        [Route("ProcessAzureQueue()")]
        public async Task<IActionResult> ProcessAzureQueue([FromBody] JObject request)
        {
            InitializeEnvironment();
            try
            {
                

                var subClient = SubscriptionClient.CreateFromConnectionString(_connectionString, _topicName, _subscriptionName);
                subClient.OnMessage(m =>
                {
                    Log.Information("Processing Azure Queue message: {m.GetBody<string>()}");
                });
                var messages = subClient.ReceiveBatch(_maxMessagesCount);
                if (messages != null && messages.Count() > 0)
                {
                    var command = Command<ImportSellableItemFromContentHubCommand>();
                    var mappingPolicy = CurrentContext.GetPolicy<SellableItemMappingPolicy>();
                    foreach (var message in messages)
                    {
                        if (message != null && message.Properties.ContainsKey("target_id"))
                        {
                            var argument = new ImportCatalogEntityArgument(mappingPolicy, typeof(SellableItem))
                            {
                                ContentHubEntityId = (string)message.Properties["target_id"]
                            };
                            var result = await command.Process(CurrentContext, argument).ConfigureAwait(false);

                            //TODO: if success
                            message.Complete();
                        }
                    } 
                }
                

                return new ObjectResult(true);
                //return result != null ? new ObjectResult(result) : new NotFoundObjectResult("Error importing SellableItem data");
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