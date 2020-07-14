using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Plugin.Sync.Commerce.CatalogImport.Commands;
using Plugin.Sync.Commerce.CatalogImport.Pipelines.Arguments;
using Plugin.Sync.Commerce.CatalogImport.Policies;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.Core.Commands;
using Sitecore.Commerce.Plugin.Catalog;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using System.Globalization;
using Microsoft.AspNetCore.Http;
using System.Configuration;
using System.Linq;
using Microsoft.Azure.Amqp.Framing;
using System.Management.Instrumentation;

namespace Plugin.Sync.Commerce.CatalogImport.ServiceBus
{
    public class ServiceBusConsumer : IServiceBusConsumer
    {
        //static string _connectionString = "Endpoint=sb://xccontenthubdemo.servicebus.windows.net/;SharedAccessKeyName=products_content;SharedAccessKey=yK0BD0C+ijNNWfl3cUJdmOJgC4MT/QSZZFqAAir7MZQ=;EntityPath=products_content";
        //static string _connectionString = "Endpoint=sb://xccontenthubdemo.servicebus.windows.net/;SharedAccessKeyName=products_content;SharedAccessKey=yK0BD0C+ijNNWfl3cUJdmOJgC4MT/QSZZFqAAir7MZQ=";
        //static string _connectionString = "Endpoint=sb://xccontenthubdemo.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=HxyFUilEe7vGB0gXAYPnBUVQA8YaG63ElEJPkaJ5Pe4=";
        static string _connectionString = "Endpoint=sb://xccontenthubdemo.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=HxyFUilEe7vGB0gXAYPnBUVQA8YaG63ElEJPkaJ5Pe4=";
        //static string _subscriptionName = "quadfectaproductsync";
        static string _topicName = "xccontenthubdemoqueue"; //"products_content";
        //static string _productEntityDefinition = "M.PCM.Product";

        //private readonly IProcessData _processData;
        private readonly IConfiguration _configuration;
        private readonly QueueClient _queueClient;
        private const string QUEUE_NAME = "simplequeue";
        private readonly ILogger _logger;
        private Plugin.Sync.Commerce.CatalogImport.Controllers.CommandsController _commandsController;
        CommerceCommander _commerceCommander;
        GetEnvironmentCommand _getEnvironmentCommand;
        ImportSellableItemFromContentHubCommand _importSellableItemFromContentHubCommand;
        ImportCategoryFromContentHubCommand _importCategoryFromContentHubCommand;
        protected internal IServiceProvider _serviceProvider { get; }
        private readonly NodeContext _nodeContext;

        CommerceEnvironment _globalEnvironment;

        public ServiceBusConsumer(//IProcessData processData,
            Plugin.Sync.Commerce.CatalogImport.Controllers.CommandsController commandsController,
            CommerceCommander commerceCommander,
            GetEnvironmentCommand getEnvironmentCommand,
            ImportSellableItemFromContentHubCommand importSellableItemFromContentHubCommand,
            ImportCategoryFromContentHubCommand importCategoryFromContentHubCommand,
            IConfiguration configuration,
            ILogger<ServiceBusConsumer> logger,
            IServiceProvider serviceProvider,
            CommerceEnvironment globalEnvironment)
        {
            _serviceProvider = serviceProvider;
            _globalEnvironment = globalEnvironment;
            _getEnvironmentCommand = getEnvironmentCommand;
            _importSellableItemFromContentHubCommand = importSellableItemFromContentHubCommand;
            _importCategoryFromContentHubCommand = importCategoryFromContentHubCommand;
            _commerceCommander = commerceCommander;
            //_processData = processData;
            _commandsController = commandsController;
            _configuration = configuration;
            _logger = logger;
            //_queueClient = new QueueClient(_configuration.GetConnectionString("ServiceBusConnectionString"), QUEUE_NAME);
            _queueClient = new QueueClient(_connectionString, _topicName);
            this._nodeContext = serviceProvider.GetService<NodeContext>();
        }

        public void RegisterOnMessageHandlerAndReceiveMessages()
        {
            var messageHandlerOptions = new MessageHandlerOptions(ExceptionReceivedHandler)
            {
                MaxConcurrentCalls = 1,
                AutoComplete = false,

            };

            _queueClient.RegisterMessageHandler(ProcessMessagesAsync, messageHandlerOptions);
        }

        private async Task ProcessMessagesAsync(Message message, CancellationToken token)
        {
            var messageString = Encoding.UTF8.GetString(message.Body);
            _logger.LogInformation($"Received Message: {messageString}");

            if (message != null
                && message.UserProperties.ContainsKey("target_id")
                && message.UserProperties.ContainsKey("target_definition")
                && message.UserProperties.ContainsKey("target_definition"))
            {
                var targetId = (string)message.UserProperties["target_id"];
                var targetDefinition = (string)message.UserProperties["target_definition"];
                var instanceName = (string)message.UserProperties["instance_name"];
                if (!string.IsNullOrEmpty(targetId) && !string.IsNullOrEmpty(targetDefinition) && !string.IsNullOrEmpty(instanceName))
                {
                    var context = GetCommerceContext();
                    var environment = await _getEnvironmentCommand.Process(context, "HabitatAuthoring").ConfigureAwait(false);
                    context.PipelineContextOptions.CommerceContext.Environment = environment;

                    var mappingPolicy = context.GetPolicy<SellableItemMappingPolicy>();
                    var mappingConfiguration = mappingPolicy?.MappingConfigurations?.FirstOrDefault(c => c.EntityType.Equals(targetDefinition, StringComparison.OrdinalIgnoreCase)
                                                                                                             && c.SourceName.Equals(instanceName, StringComparison.OrdinalIgnoreCase));

                    ImportCatalogEntityArgument result = null;
                    if (mappingConfiguration != null)
                    {
                        result = await TryProcessSellableItem(targetId, targetDefinition, mappingConfiguration, context).ConfigureAwait(false);
                    }
                    else
                    {
                        var categoryMappingPolicy = context.GetPolicy<CategoryMappingPolicy>();
                        mappingConfiguration = categoryMappingPolicy?.MappingConfigurations?.FirstOrDefault(c => c.EntityType.Equals(targetDefinition, StringComparison.OrdinalIgnoreCase)
                                                                                                             && c.SourceName.Equals(instanceName, StringComparison.OrdinalIgnoreCase));
                        result = await TryProcessCategory(targetId, targetDefinition, mappingConfiguration, context).ConfigureAwait(false);
                    }
                    if (result == null)
                    {
                        _logger.LogError($"Cannot process Service Bus message. Mapping configuration not found for EntityType=={targetDefinition} and SourceName=={instanceName}");
                    }
                }
            }
            else
            {
                _logger.LogError($"Cannot process Service Bus message. UserProperties: {string.Join(Environment.NewLine, message.UserProperties)}. Message: {messageString}");
            }

            await _queueClient.CompleteAsync(message.SystemProperties.LockToken);
        }

        private async Task<ImportCatalogEntityArgument> TryProcessSellableItem(string sourceId, string sourceEntityType, MappingConfiguration mappingConfiguration, CommerceContext context)
        {
            var argument = new ImportCatalogEntityArgument(mappingConfiguration, typeof(SellableItem))
            {
                ContentHubEntityId = sourceId,
                SourceEntityType = sourceEntityType,
            };
            return await _importSellableItemFromContentHubCommand.Process(context, argument).ConfigureAwait(false);
        }

        private async Task<ImportCatalogEntityArgument> TryProcessCategory(string sourceId, string sourceEntityType, MappingConfiguration mappingConfiguration, CommerceContext context)
        {
            var argument = new ImportCatalogEntityArgument(mappingConfiguration, typeof(SellableItem))
            {
                ContentHubEntityId = sourceId,
                SourceEntityType = sourceEntityType
            };
            //var command = _serviceProvider.GetService<ImportCategoryFromContentHubCommand>();
            return await _importCategoryFromContentHubCommand.Process(context, argument).ConfigureAwait(false);
        }


        private CommerceContext GetCommerceContext()
        {
            var logger = _logger;// (ILogger)_serviceProvider.GetService<ILogger<CommerceController>>();
            var _nodeContext = _serviceProvider.GetService<NodeContext>();
            ITrackActivityPipeline service = _serviceProvider.GetService<ITrackActivityPipeline>();
            CommerceContext commerceContext = new CommerceContext(logger, new Microsoft.ApplicationInsights.TelemetryClient());//, _serviceProvider.GetService<IGetLocalizableMessagePipeline>());
            commerceContext.GlobalEnvironment = _globalEnvironment;
            commerceContext.ConnectionId = Guid.NewGuid().ToString("N", (IFormatProvider)CultureInfo.InvariantCulture);
            commerceContext.CorrelationId = Guid.NewGuid().ToString("N", (IFormatProvider)CultureInfo.InvariantCulture);
            commerceContext.TrackActivityPipeline = service;
            //NodeContext nodeContext = _nodeContext;
            commerceContext.PipelineTraceLoggingEnabled = _nodeContext != null && _nodeContext.PipelineTraceLoggingEnabled;

            commerceContext.Headers = new HeaderDictionary();
            commerceContext.Headers.Add("Roles", @"sitecore\Commerce Administrator|sitecore\Customer Service Representative Administrator|sitecore\Customer Service Representative|sitecore\Commerce Business User|sitecore\Pricer Manager|sitecore\Pricer|sitecore\Promotioner Manager|sitecore\Promotioner|sitecore\Merchandiser|sitecore\Relationship Administrator");
            //commerceContext.Headers.Add("Language", "en-US");
            //commerceContext.Headers.Add("Environment", "HabitatAuthoring");
            commerceContext.Environment = this._nodeContext?.GetEntity<CommerceEnvironment>((Func<CommerceEnvironment, bool>)(e => e.Name.Equals("HabitatAuthoring", StringComparison.OrdinalIgnoreCase)))
                        ?? Task.Run<CommerceEnvironment>((Func<Task<CommerceEnvironment>>)(() => _getEnvironmentCommand?.Process(commerceContext, "HabitatAuthoring"))).Result;
            if (commerceContext.Environment == null)
            {
                commerceContext.Environment = _globalEnvironment;
            }
            return commerceContext;


            //this._baseContext = commerceContext;
        }

        private Task ExceptionReceivedHandler(ExceptionReceivedEventArgs exceptionReceivedEventArgs)
        {
            _logger.LogError(exceptionReceivedEventArgs.Exception, "Message handler encountered an exception");
            var context = exceptionReceivedEventArgs.ExceptionReceivedContext;

            _logger.LogDebug($"- Endpoint: {context.Endpoint}");
            _logger.LogDebug($"- Entity Path: {context.EntityPath}");
            _logger.LogDebug($"- Executing Action: {context.Action}");

            return Task.CompletedTask;
        }

        public async Task CloseQueueAsync()
        {
            await _queueClient.CloseAsync();
        }
    }
}