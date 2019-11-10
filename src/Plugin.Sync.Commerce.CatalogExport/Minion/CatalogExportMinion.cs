using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Plugin.Sync.Commerce.CatalogImport.Policies;
using Plugin.Sync.Commerce.CatalogExport.Pipelines;
using Plugin.Sync.Commerce.CatalogExport.Pipelines.Arguments;
using Plugin.Sync.Commerce.CatalogExport.Policies;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.Plugin.Catalog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Plugin.Sync.Commerce.CatalogExport.Minion
{

    public class CatalogExportMinion : Sitecore.Commerce.Core.Minion
    {
        protected IExportProductMinionPipeline MinionPipeline { get; set; }
        protected IRemoveListEntitiesPipeline _removeListEntitiesPipeline { get; set; }
        protected IAddListEntitiesPipeline _addListEntitiesPipeline { get; set; }
        protected IFindEntitiesInListPipeline _findEntitiesInListPipeline { get; set; }
       // private int isRunning = 0;

        public override void Initialize(IServiceProvider serviceProvider, ILogger logger, MinionPolicy policy, CommerceEnvironment environment,
            CommerceContext globalContext)
        {
            base.Initialize(serviceProvider, logger, policy, environment, globalContext);
            MinionPipeline = serviceProvider.GetService<IExportProductMinionPipeline>();
            _removeListEntitiesPipeline = serviceProvider.GetService<IRemoveListEntitiesPipeline>();
            _addListEntitiesPipeline = serviceProvider.GetService<IAddListEntitiesPipeline>();

        }

        [Obsolete("This method is deprecated, use Execute instead.")]
        public override Task<MinionRunResultsModel> Run()
        {
            throw new System.NotImplementedException();
        }

        protected override async Task<MinionRunResultsModel> Execute()
        {
            var minionResult = new MinionRunResultsModel();
            //if (Interlocked.CompareExchange(ref isRunning, 1, 0) != 0)
            //{
            //    Logger.LogInformation($"ExportCatalogMinion: minion is allready running!");
            //    minionResult.DidRun = false;
            //    throw new Exception("SyncProductToChannelAdvisorMinion:Cannot execute, minion is allready running");
            //}
            try
            {
                Logger.LogInformation($"ExportCatalogMinion: minion run started at {DateTime.UtcNow}");

                var commerceContext = new CommerceContext(this.Logger, this.MinionContext.TelemetryClient, null);
                commerceContext.Environment = this.Environment;
                CommercePipelineExecutionContextOptions executionContextOptions = new CommercePipelineExecutionContextOptions(commerceContext, null, null, null, null, null);

                var sellableItemPolicy = commerceContext.PipelineContext.GetPolicy<SellableItemMappingPolicy>();

                // Process UpdatedSellableItemsList
                // long count = await GetListCount(sellableItemPolicy.UpdatedSellableItemsList)
                var readSellableItems = await GetListItems<SellableItem>(sellableItemPolicy.UpdatedSellableItemsList, Convert.ToInt32(this.Policy.ItemsPerBatch));
                
                if (readSellableItems.OfType<SellableItem>().Count() > 0L)
                {
                    //if (count > this.Policy.ItemsPerBatch)
                    //{
                    //    count = this.Policy.ItemsPerBatch;
                    //}
                    await SyncProducts(sellableItemPolicy, readSellableItems.OfType<SellableItem>().ToList<SellableItem>(), commerceContext, executionContextOptions);
                }
                else
                {
                    Logger.LogInformation(string.Format("{0} No items available to sync with channel advisor", Name), Array.Empty<object>());
                }

                Logger.LogInformation($"ExportCatalogMinion completed at {DateTime.UtcNow}");
            }
            catch (Exception ex)
            {
                Logger.LogError($"ExportCatalogMinion Exception thrown '. Error: {ex.Message}. StackTrace: {ex.StackTrace}", Array.Empty<object>());
            }
            finally
            {
                minionResult.DidRun = true;
             //   isRunning = 0;
            }
          
            return minionResult;
        }

        private async Task<bool> SyncProducts(SellableItemMappingPolicy sellableItemPolicy, List<SellableItem> sellableItems, CommerceContext commerceContext, CommercePipelineExecutionContextOptions executionContextOptions)
        {
            Logger.LogInformation(string.Format("{0}-Fetching {1} list from commerce for processing, Total Item(s) {2} ", Name, sellableItemPolicy.UpdatedSellableItemsList, sellableItems.Count), Array.Empty<object>());
            // var sellableItems = (await GetListItems<SellableItem>(sellableItemPolicy.UpdatedSellableItemsList, Convert.ToInt32(sellableItemPolicy.batch)));

            //var client = new ClientHelper(commerceContext, Logger);
            //client.RefreshAccessToken();

            //var profileId = commerceContext.PipelineContext.GetPolicy<CatalogExportClienPolicy>()?.ProfileId;
            //var failedCount = 0;
            //int seqno = 1;
            //// executing  each sellable item for processing
            //foreach (var item in sellableItems)
            //{
            //    try
            //    {
            //        client._PerItemExecutionCalls_ToCA = 0;
            //        Logger.LogInformation(string.Format("Seq No :{2}, {0}-Starting to push item to channel advisor: {1}", Name, item.Id, seqno), Array.Empty<object>());

            //        var arg = new CatalogExportArgument(item);
            //        arg.ProfileId = profileId;
            //        arg.SellableItemMappingPolicy = sellableItemPolicy;
            //        var result = await this.MinionPipeline.Run(arg, executionContextOptions);
            //        Logger.LogInformation(string.Format("Seq No :{2}, Total {0} Api Calls to channel advisor for Item: {1}", client._PerItemExecutionCalls_ToCA, item.Id, seqno), Array.Empty<object>());
            //        seqno++;
            //        ListEntitiesArgument listArgument = null;
            //        ListEntitiesArgument addToListResult = null;
            //        ListEntitiesArgument removeFromListResult = null;

            //        if (result != null && (result.IsSuccesful || result.IsAlreadyDeleted))
            //        {
            //            // Add item into ProcessSellableItemsList
            //            listArgument = new ListEntitiesArgument(new string[1] { item.Id }, sellableItemPolicy.ProcessSellableItemsList);
            //            addToListResult = await this._addListEntitiesPipeline.Run(listArgument, executionContextOptions);

            //            // remove  item from UpdatedSellableItemsList list
            //            listArgument = new ListEntitiesArgument(new string[1] { item.Id }, sellableItemPolicy.UpdatedSellableItemsList);
            //            removeFromListResult = await this._removeListEntitiesPipeline.Run(listArgument, executionContextOptions);

            //            Logger.LogInformation(string.Format("{0}-Syncd successfully item: {1}", Name, item.Id), Array.Empty<object>());
            //            continue;
            //        }

            //        // Add item into FailedSellableItemsListName 
            //        listArgument = new ListEntitiesArgument(new string[1] { item.Id }, sellableItemPolicy.FailedSellableItemsList);
            //        addToListResult = await this._addListEntitiesPipeline.Run(listArgument, executionContextOptions);

            //        failedCount++;
            //        Logger.LogInformation($"{Name}: Error returned from CA for item '{item.Id}'. StatusCode: {result?.ResponseStatusCode.ToString()}. Error: {result?.ErrorMessage}. Response: {result?.ResponseContent}", Array.Empty<object>());
            //    }
            //    catch (Exception ex)
            //    {
            //        failedCount++;
            //        Logger.LogError($"{Name}: Exception thrown when try to push item to channel advisor '{item.Id}'. Error: {ex.Message}. StackTrace: {ex.StackTrace}", Array.Empty<object>());
            //    }
            //}
            //client.WriteFinalExecutionCount();
            //Logger.LogInformation($"Successfully processed {sellableItems.Count - failedCount} sellable items. Failed processing {failedCount} sellable items.");

            await Task.CompletedTask;
            throw new NotImplementedException();
        }
    }
}