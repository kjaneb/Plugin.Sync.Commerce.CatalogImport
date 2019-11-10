using Microsoft.Extensions.Logging;
using Plugin.Sync.Commerce.CatalogImport.Helpers;
using Plugin.Sync.Commerce.CatalogExport.Pipelines.Arguments;
using RestSharp;
using Sitecore.Commerce.Core;
using Sitecore.Framework.Pipelines;
using System.Linq;
using System.Threading.Tasks;

namespace Plugin.Sync.Commerce.CatalogExport.Pipelines.Blocks
{
    [PipelineDisplayName("CatalogExportMinionBlock")]
    public class CatalogExportMinionBlock : PipelineBlock<CatalogExportArgument, CatalogExportArgument, CommercePipelineExecutionContext>
    {

        public CatalogExportMinionBlock(IFindEntitiesInListPipeline findEntitiesInListPipeline)
        {
        }

        public override async Task<CatalogExportArgument> Run(CatalogExportArgument arg, CommercePipelineExecutionContext context)
        {
            //var requestProduct = CAMapper.ConvertSellableItemToProductRequest(arg.CurrentItem, arg.ProfileId);
            //IRestResponse response = null;
           
            ////add or update basic info 
            //response = arg.ClientHelper.SyncProduct(requestProduct);
            
            ////if unauthorized mean token is expired or some aother authentication problem
            //if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            //{
            //    //try once more
            //    arg.ClientHelper.RefreshAccessToken();
            //    response = arg.ClientHelper.SyncProduct(requestProduct);

            //    //still unauthorized then log error and abort 
            //    if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            //    {
            //        string error = string.Format($"{this.Name}: Authorization failed second time.");
            //        context.Logger.LogInformation(error);
            //        context.Abort(error, context);
            //        arg.ResponseStatusCode = System.Net.HttpStatusCode.Unauthorized;
            //        return arg;
            //    }
            //}

            //arg=UpdateReturnedArgument(response, arg);
            ////if not successful then return, dont need to sync attributes and images;
            //if (!arg.IsSuccesful || requestProduct.ID < 1)
            //{
            //    context.Logger.LogError($"ExportCatalogMinionBlock: Failed to update basic info for product id: {arg.CurrentItem.ProductId}, StatusCode: {response.StatusCode}. Response : {response.Content}");
            //    return arg;
            //}

            ////remove attributes
            //context.Logger.LogInformation($"ExportCatalogMinionBlock: Removing attributes for product id: {arg.CurrentItem.ProductId}, name: {arg.CurrentItem.Name}");
            //response = arg.ClientHelper.RemoveProductAttributes(CommerceCatalogHelper.GetComposerDisctionary(arg.CurrentItem), requestProduct.ID.ToString());
            //arg=UpdateReturnedArgument(response, arg);
            //if (!arg.IsSuccesful)
            //{
            //    context.Logger.LogError($"ExportCatalogMinionBlock: Failed to remove attributes for product id: {arg.CurrentItem.ProductId}, StatusCode: {response.StatusCode}");
            //    return arg;
            //}
            
            //// Update attributes
            //context.Logger.LogInformation($"ExportCatalogMinionBlock: Updating attributes for product id: {arg.CurrentItem.ProductId}, name: {arg.CurrentItem.Name}");
            //response = arg.ClientHelper.SyncProductAttributes(CommerceCatalogHelper.GetComposerDisctionary(arg.CurrentItem), requestProduct.ID.ToString());
            //arg=UpdateReturnedArgument(response, arg);
            //if (!arg.IsSuccesful)
            //{
            //    context.Logger.LogError($"ExportCatalogMinionBlock: Failed to update attributes for product id: {arg.CurrentItem.ProductId}, StatusCode: {response.StatusCode}");
            //    return arg;
            //}

            ////remove all images
            //context.Logger.LogInformation($"ExportCatalogMinionBlock: Removing images for product id: {arg.CurrentItem.ProductId}, name: {arg.CurrentItem.Name}");
            //response = arg.ClientHelper.RemoveProductImages( requestProduct.ID.ToString());
            //arg=UpdateReturnedArgument(response, arg);
            //if (!arg.IsSuccesful)
            //{
            //    context.Logger.LogError($"ExportCatalogMinionBlock: Failed to remove images for product id: {arg.CurrentItem.ProductId}, StatusCode: {response.StatusCode}");
            //    return arg;
            //}

            //// Update images
            //context.Logger.LogInformation($"ExportCatalogMinionBlock: Updating images for product id: {arg.CurrentItem.ProductId}, name: {arg.CurrentItem.Name}");
            //response = arg.ClientHelper.UpdateProductImages(arg.CurrentItem,requestProduct.ID.ToString());
            //arg = UpdateReturnedArgument(response, arg);
            //if (!arg.IsSuccesful)
            //{
            //    context.Logger.LogError($"ExportCatalogMinionBlock: Failed to update images for product id: {arg.CurrentItem.ProductId}, StatusCode: {response.StatusCode}");
            //    return arg;
            //}
            
            return await Task.FromResult(arg);
        }

        private CatalogExportArgument UpdateReturnedArgument(IRestResponse response, CatalogExportArgument arg)
        {
            if (response == null)
            {
                arg.IsSuccesful = false;
                return arg;
            }
            arg.IsSuccesful = response.IsSuccessful;
            arg.ErrorMessage = response.ErrorMessage;
            arg.ResponseContent = response.Content;
            arg.ResponseStatusCode = response.StatusCode;
            return arg;
        }
    }
}