//using Newtonsoft.Json.Linq;
//using Plugin.Sync.Commerce.CatalogImport.Entities;
//using Plugin.Sync.Commerce.CatalogImport.Extensions;
//using Plugin.Sync.Commerce.CatalogImport.Helpers;
//using Plugin.Sync.Commerce.CatalogImport.Models;
//using Plugin.Sync.Commerce.CatalogImport.Pipelines.Arguments;
//using Plugin.Sync.Commerce.CatalogImport.Pipelines.Helper;
//using Plugin.Sync.Commerce.CatalogImport.Policies;
//using Serilog;
//using Sitecore.Commerce.Core;
//using Sitecore.Commerce.Core.Commands;
//using Sitecore.Commerce.EntityViews;
//using Sitecore.Commerce.EntityViews.Commands;
//using Sitecore.Commerce.Plugin.Catalog;
//using Sitecore.Commerce.Plugin.Composer;
//using Sitecore.Commerce.Plugin.Management;
//using Sitecore.Commerce.Plugin.Pricing;
//using Sitecore.Commerce.Plugin.Views;
//using Sitecore.Framework.Pipelines;
//using System;
//using System.Collections;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;
//using Microsoft.Extensions.Logging;
//using Sitecore.Commerce.Plugin.Workflow;
//using Newtonsoft.Json;
//using Sitecore.Framework.Conditions;

//namespace Plugin.Sync.Commerce.CatalogImport.Pipelines.Blocks
//{
  

//    [PipelineDisplayName("ParseCommerceEntityData")]
//    public class ParseCatalogEntityJsonDataBlock : PipelineBlock<ImportSellableItemArgument, SellableItemResponse, CommercePipelineExecutionContext>
//    {
//        private readonly CommerceCommander _commerceCommander;
//        private readonly ComposerCommander _composerCommander;
//        private readonly FindEntityCommand _findEntityCommand;
//        private readonly CreateSellableItemCommand _createSellableItemCommand;

//        private readonly GetEntityViewCommand _getEntityViewCommand;
//        private readonly DoActionCommand _doActionCommand;
//        private SitecoreContentReader _sitecoreContentReader;
//        private readonly IRemoveListEntitiesPipeline _removeListEntitiesPipeline;
//        private readonly IAddListEntitiesPipeline _addListEntitiesPipeline;
//        private readonly IFindEntitiesInListPipeline _findEntitiesInListPipeline;

        

//        public ParseCatalogEntityJsonDataBlock()
//        {
//        }
//        public override async bool Run(ImportSellableItemArgument arg, CommercePipelineExecutionContext context)
//        {
//            var mappingPolicy = context.CommerceContext.GetPolicy<CatalogEntityMappingPolicy>();
//            //TODO: thrown maningful error if policy is empty or incomplete
//            Condition.Requires(arg, nameof(arg)).IsNotNull();
//            Condition.Requires(arg.JsonData, nameof(arg.JsonData)).IsNotNull();
//            arg.EntityData = new CommerceEntityData();
//            arg.EntityData.Id = arg.JsonData.SelectValueOrDefault<string>(mappingPolicy.IdPath);
//            arg.EntityData.Name = arg.JsonData.SelectValueOrDefault<string>(mappingPolicy.NamePath);
//            arg.EntityData.CatalogName = arg.JsonData.SelectValueOrDefault<string>(mappingPolicy.CatalogNamePath);
//            arg.EntityData.ParentCategoryName = arg.JsonData.SelectValueOrDefault<string>(mappingPolicy.ParentCategoryNamePath);
//            arg.EntityData.DisplayName = arg.JsonData.SelectValueOrDefault<string>(mappingPolicy.DisplayNamePath);
//            arg.EntityData.Description = arg.JsonData.SelectValueOrDefault<string>(mappingPolicy.DescriptionPath);

//            Condition.Requires(arg.EntityData.Id, nameof(arg.EntityData.Id)).IsNotNullOrEmpty();
//            Condition.Requires(arg.EntityData.Name, nameof(arg.EntityData.Name)).IsNotNullOrEmpty();

//            //arg.EntityData.Fields = 
//            return true;
//        }

//        public async Dictionary<string, string> ParseFields(CatalogEntityMappingPolicy mappingPolicy, JObject jsonData)
//        {

//        }

//        public async Task<SellableItemResponse> SyncVehicleData(ImportSellableItemArgument arg, CommercePipelineExecutionContext context)
//        {
//            SellableItemResponse response = new SellableItemResponse();
//            try
//            {
//                Dictionary<string, string> inputData = arg.SellableItemFeatures;

//                Dictionary<string, string> sellableItemImages = arg.SellableItemImages;
//                var commerceCatalogId = CommerceCatalogHelper.GetCommerceCatalogId(inputData["CatalogId"]);

//                var catalog = Task.Run<CommerceEntity>(async () => await _findEntityCommand.Process(context.CommerceContext, typeof(Catalog), commerceCatalogId)).Result as Catalog;
//                if (catalog == null)
//                {
//                    throw new ArgumentException($"Catalog not found, CatalogId: {inputData["CatalogId"]}");
//                }

//                var category = await _commerceCommander.Command<GetCategoryCommand>()
//                    .Process(context.CommerceContext, inputData["CategoryId"].ToCategoryFriendlyId(inputData["CatalogId"]));

//                if (category == null)
//                {
//                    throw new ArgumentException($"Parent category not found, CategoryId: {inputData["CategoryId"]}");
//                }

//                var commerceSellableItemId = CommerceCatalogHelper.GetCommerceSellableItemId(inputData["Id"]);
//                response = await CreateOrGetUpdatedSellableItem(inputData, commerceSellableItemId, context.CommerceContext);

//                //Sync composer fields and images
//                var isUpdated = await SyncChildViewsAndImages(sellableItemImages, inputData, context.CommerceContext, response.SellableItem, arg);

//                if (isUpdated)
//                {
//                    response.SellableItem = await _findEntityCommand.Process(context.CommerceContext, typeof(SellableItem), commerceSellableItemId) as SellableItem;
//                }

//                //clear list price and re-add again.
//                response.AddListPrice(CommerceCatalogHelper.GetListPriceFromPriceCollection(arg.Prices));

//                var persistResult = await _commerceCommander.Pipeline<IPersistEntityPipeline>().Run(new PersistEntityArgument(response.SellableItem), context.CommerceContext.PipelineContextOptions);
//                response.SellableItem = persistResult.Entity as SellableItem;

//                if (string.IsNullOrEmpty(response.SellableItem.ParentCategoryList) || !response.SellableItem.ParentCategoryList.Contains(category.SitecoreId))
//                {
//                    //await _commerceCommander.Command<DeleteRelationshipCommand>().Process(context.CommerceContext, oldParentCategory.Id, item.Id, "CategoryToSellableItem")).Result;
//                    var sellableItemAssociation = await _commerceCommander.Command<AssociateSellableItemToParentCommand>().Process(context.CommerceContext, inputData["CategoryId"], category.Id, response.SellableItem.Id);
//                }

//                return response;
//            }
//            catch (Exception ex)
//            {
//                context.Abort($"SyncSellableItemBlock.SyncVehicleData(): Failed to create/update SellableItem, Error Message: {ex.Message}", context);
//                response.ErrorMessage = ex.Message;
//                return response;
//            }
//        }

//        private Dictionary<string, string> GetFeatureData(string sitecoreContentRootPath, CommercePipelineExecutionContext context, JObject requestJson)
//        {
//            var itemModel = _sitecoreContentReader.GetSitecoreItem(context, sitecoreContentRootPath);
//            var childrenItems = _sitecoreContentReader.GetSitecoreChildrenItems(context, itemModel["ItemID"].ToString());
//            var featureData = new Dictionary<string, string>();

//            foreach (var item in childrenItems)
//            {
//                var value = (string)requestJson.SelectToken(item["Value Json Path"].ToString());
//                if (value != null)
//                {
//                    featureData.Add(item["Field Name"].ToString(), value);
//                }
//            }
//            return featureData;
//        }

//        private List<SitecoreFieldMapping> GetDictionaryTranslationSettings(string sitecoreContentRootPath, CommercePipelineExecutionContext context)
//        {
//            var itemModel = _sitecoreContentReader.GetSitecoreItem(context, sitecoreContentRootPath);
//            var fields = new List<SitecoreFieldMapping>();
//            if (itemModel != null)
//            {
//                var fieldsSyncSettings = _sitecoreContentReader.GetSitecoreChildrenItems(context, itemModel["ItemID"].ToString());

//                foreach (var field in fieldsSyncSettings)
//                {
//                    //  if (!field["Do Not Overwrite"].ToString().Equals("1", StringComparison.CurrentCultureIgnoreCase) && field["HasChildren"].ToString().Equals("true", StringComparison.CurrentCultureIgnoreCase))



//                    if (field["HasChildren"].ToString().Equals("true", StringComparison.CurrentCultureIgnoreCase))
//                    {
//                        var dictionaries = _sitecoreContentReader.GetSitecoreChildrenItems(context, field["ItemID"].ToString());
//                        var dictionaryMapping = new List<SitecoreDictionaryMapping>();
//                        foreach (var dic in dictionaries)
//                        {
//                            if (!dic.ContainsKey("Substring Lookup"))
//                            {
//                                Log.Information($"Recursive Template not allow in {sitecoreContentRootPath}");
//                                continue;
//                            }

//                            dictionaryMapping.Add(new SitecoreDictionaryMapping
//                            {
//                                ItemName = dic["ItemName"].ToString(),
//                                FromValue = dic["From Value"].ToString(),
//                                ToValue = dic["To Value"].ToString(),
//                                SubstringLookup = dic["Substring Lookup"].ToString().Equals("1", StringComparison.CurrentCultureIgnoreCase) ? true : false
//                            });
//                        }
//                        fields.Add(new SitecoreFieldMapping
//                        {
//                            ItemName = field["ItemName"].ToString(),
//                            FieldName = field["Field Name"].ToString(),
//                            ValueJsonPath = field["Value Json Path"].ToString(),
//                            DictionaryMappings = dictionaryMapping,
//                            DoNotOverwrite = field["Do Not Overwrite"].ToString().Equals("1", StringComparison.CurrentCultureIgnoreCase)
//                        });
//                    }
//                }
//            }
//            return fields;
//        }

//        private async Task<bool> SyncChildViewsAndImages(Dictionary<string, string> sellableItemImages, Dictionary<string, string> inputData, CommerceContext commerceContext, SellableItem sellableItem, ImportSellableItemArgument arg)
//        {

//            var masterView = await _getEntityViewCommand.Process(commerceContext, sellableItem.Id, sellableItem.EntityVersion, commerceContext.GetPolicy<KnownCatalogViewsPolicy>().Master, "", "");
//            if (masterView == null)
//            {
//                Log.Error($"Master view not found on Sellable Item entity, Entity ID={sellableItem.Id}");
//                throw new ApplicationException($"Master view not found on Sellable Item entity, Entity ID={sellableItem.Id}");
//            }

//            if (masterView.ChildViews == null || masterView.ChildViews.Count == 0)
//            {
//                Log.Error($"No composer-generated views found on Sellable Item entity, Entity ID={sellableItem.Id}");
//                throw new ApplicationException($"No composer-generated views found on Sellable Item entity, Entity ID={sellableItem.Id}");
//            }

//            var sitecoreFields = GetDictionaryTranslationSettings(commerceContext.GetPolicy<SellableItemMappingPolicy>()?.SitecoreFieldSyncSettings, commerceContext.PipelineContext);
//            var isUpdated = false;
//            foreach (var view in masterView.ChildViews)
//            {
//                var viewToUpdate = masterView.ChildViews.FirstOrDefault(e => e.Name.Equals(view.Name, StringComparison.OrdinalIgnoreCase)) as EntityView;
//                if (viewToUpdate != null)
//                {
//                    var composerViewForEdit = Task.Run<EntityView>(async () => await sellableItem.GetComposerView(viewToUpdate.ItemId, _commerceCommander, commerceContext)).Result;
//                    if (composerViewForEdit != null)
//                    {
//                        foreach (var fieldProperty in composerViewForEdit.Properties)
//                        {
//                            var inputDataField = inputData.Where(x => x.Key.ToLower().Equals(fieldProperty.Name.ToLower(), StringComparison.CurrentCultureIgnoreCase)).FirstOrDefault();

//                            if (inputDataField.Key != null)
//                            {
//                                string composerVal = "";
//                                if (fieldProperty.RawValue != null)
//                                {
//                                    composerVal = fieldProperty.RawValue.ToString();
//                                }
//                                else if (fieldProperty.Value != null)
//                                {
//                                    composerVal = fieldProperty.Value.ToString();
//                                }

//                                var jsonValue = inputDataField.Value; // inputData[fieldProperty.Name];

//                                if (!string.IsNullOrEmpty(jsonValue))
//                                {
//                                    var composerFieldName = fieldProperty.Name;

//                                    var siteCoreFieldMatch = sitecoreFields.FirstOrDefault(x => x.ItemName.ToLower().Equals(composerFieldName.ToLower(), StringComparison.CurrentCultureIgnoreCase));

//                                    // There is a field match
//                                    if (siteCoreFieldMatch != null)
//                                    {

//                                        // Do Not Overwrite is checked
//                                        if (siteCoreFieldMatch.DoNotOverwrite)
//                                        {
//                                            // Do Not Overwrite has initial value, use jsonvalue
//                                            if (!string.IsNullOrEmpty(composerVal.Trim()))
//                                            {
//                                                fieldProperty.ParseValueAndSetEntityView(jsonValue, arg);
//                                                isUpdated = true;
//                                            }
//                                            else
//                                            {
//                                                // Do Not Overwrite does not has initial value, so, create value
//                                                var dicMappedValues = siteCoreFieldMatch.DictionaryMappings.ToList();
//                                                if (dicMappedValues.Any())
//                                                {
//                                                    var mappingFound = false;

//                                                    foreach (var dictionaryMapping in dicMappedValues)
//                                                    {
//                                                        if (dictionaryMapping?.FromValue != null &&
//                                                            !string.IsNullOrEmpty(dictionaryMapping.FromValue) &&
//                                                            dictionaryMapping.ToValue != null &&
//                                                            !string.IsNullOrEmpty(dictionaryMapping.ToValue))
//                                                        {
//                                                            if (jsonValue.ToLower().Equals(dictionaryMapping.FromValue
//                                                                .Trim().ToLower()))
//                                                            {
//                                                                mappingFound = true;
//                                                                jsonValue = dictionaryMapping.FromValue;
//                                                                fieldProperty.ParseValueAndSetEntityView(jsonValue, arg);
//                                                                isUpdated = true;
//                                                            }
//                                                        }
//                                                    }

//                                                    if (!mappingFound)
//                                                    {
//                                                        // No mapping found value, we still need to update value with what is being synced
//                                                        fieldProperty.ParseValueAndSetEntityView(jsonValue, arg);
//                                                        isUpdated = true;
//                                                    }
//                                                }
//                                                else
//                                                {
//                                                    // No mapped value, we still need to update value with what is being synced
//                                                    fieldProperty.ParseValueAndSetEntityView(jsonValue, arg);
//                                                    isUpdated = true;
//                                                }
//                                            }
//                                        }
//                                        else
//                                        {
//                                            var dicMappedValues = siteCoreFieldMatch.DictionaryMappings.ToList();
//                                            if (dicMappedValues.Any())
//                                            {
//                                                var mappingFound = false;

//                                                foreach (var dictionaryMapping in dicMappedValues)
//                                                {
//                                                    if (dictionaryMapping?.FromValue != null &&
//                                                        !string.IsNullOrEmpty(dictionaryMapping.FromValue) &&
//                                                        dictionaryMapping.ToValue != null &&
//                                                        !string.IsNullOrEmpty(dictionaryMapping.ToValue))
//                                                    {
//                                                        if (jsonValue.ToLower().Equals(dictionaryMapping.FromValue
//                                                            .Trim().ToLower()))
//                                                        {
//                                                            mappingFound = true;
//                                                            jsonValue = dictionaryMapping.ToValue;
//                                                            fieldProperty.ParseValueAndSetEntityView(jsonValue, arg);
//                                                            isUpdated = true;
//                                                        }
//                                                    }
//                                                }

//                                                if (!mappingFound)
//                                                {
//                                                    fieldProperty.ParseValueAndSetEntityView(jsonValue, arg);
//                                                    isUpdated = true;
//                                                }
//                                            }
//                                            else
//                                            {
//                                                fieldProperty.ParseValueAndSetEntityView(jsonValue, arg);
//                                                isUpdated = true;
//                                            }

//                                        }
//                                    }
//                                    else
//                                    {
//                                        // IF NO SITECORE FIELD MATCH, still update a mathed field with value
//                                        fieldProperty.ParseValueAndSetEntityView(jsonValue, arg);
//                                        isUpdated = true;
//                                    }
//                                }
//                                else
//                                {
//                                    if (jsonValue == "")
//                                    {
//                                        fieldProperty.ParseValueAndSetEntityView(jsonValue, arg);
//                                    }
//                                    fieldProperty.Value = "";
//                                    fieldProperty.RawValue = "";
//                                    isUpdated = true;
//                                }
//                            }
//                            else
//                            {

//                                fieldProperty.Value = "";
//                                fieldProperty.RawValue = "";
//                                isUpdated = true;
//                            }
//                        }
//                    }

//                }
//            }

//            //if (sellableItem.HasComponent<CustomImagesComponent>())
//            //{
//            //    sellableItem.RemoveComponent(typeof(CustomImagesComponent));
//            //    isUpdated = true;
//            //}

//            //if (sellableItemImages.Count > 0)
//            //{
//            //    CustomImagesComponent customImagesComponent = new CustomImagesComponent();
//            //    customImagesComponent.Images = sellableItemImages;
//            //    customImagesComponent.ImagesJson = JsonConvert.SerializeObject(sellableItemImages);
//            //    sellableItem.SetComponent(customImagesComponent);
//            //    isUpdated = true;
//            //}

//            if (isUpdated)
//            {
//                await _composerCommander.PersistEntity(commerceContext, sellableItem);
//            }
//            return isUpdated;
//        }

//        private async Task<SellableItemResponse> CreateOrGetUpdatedSellableItem(Dictionary<string, string> inputData, string commerceSellableItemId, CommerceContext commerceContext)
//        {
//            WorkflowComponent workflowComponent = null;
//            var sellableItemResponse = new SellableItemResponse();
//            var sellableItem = await _findEntityCommand.Process(commerceContext, typeof(SellableItem), commerceSellableItemId) as SellableItem;
//            if (sellableItem == null)
//            {
//                var idString = inputData["Id"];
//                long id;
//                if (!long.TryParse(inputData["Id"], out id))
//                {
//                    throw new ArgumentException($"Item ID value='{inputData["Id"]}' has incorrect format - must be a number.");
//                }
//                await _createSellableItemCommand.Process(commerceContext,
//                    idString,
//                    inputData["Name"],
//                    inputData["DisplayName"],
//                    inputData.ContainsKey("Description") ? inputData["Description"] : null,
//                    inputData.ContainsKey("Brand") ? inputData["Brand"] : null,
//                    inputData.ContainsKey("Manufacturer") ? inputData["Manufacturer"] : null,
//                    inputData.ContainsKey("TypeOfGood") ? inputData["TypeOfGood"] : null,
//                    new string[] { inputData["SamClass"] });

//                sellableItem = await _findEntityCommand.Process(commerceContext, typeof(SellableItem), commerceSellableItemId) as SellableItem;

//                if (sellableItem == null)
//                {
//                    Log.Error($"Sellable item, {inputData["Id"]} | {inputData["CategoryId"]}, was not created.");
//                    throw new ApplicationException($"Sellable item, {inputData["Id"]} | {inputData["CategoryId"]}, was not created.");
//                }
//                workflowComponent = sellableItem.GetComponent<WorkflowComponent>();

//                if (workflowComponent == null)
//                {
//                    workflowComponent = new WorkflowComponent();
//                }
//                workflowComponent.Workflow = new EntityReference { EntityTarget = "Entity-Workflow-UVSCustomWorkflow", Name = "UVSCustomWorkflow" };
//                workflowComponent.CurrentState = "New";
//                sellableItem.SetComponent(workflowComponent);
//                sellableItemResponse.StatusCode = 201;
//                sellableItemResponse.SellableItem = sellableItem;
//                sellableItemResponse.IsNew = true;
//            }
//            else
//            {
//                sellableItem.Name = inputData["Name"];
//                sellableItem.DisplayName = inputData["DisplayName"];
//                sellableItem.Description = inputData.ContainsKey("Description") ? inputData["Description"] : null;
//                sellableItem.Brand = inputData.ContainsKey("Brand") ? inputData["Brand"] : null;
//                sellableItem.Manufacturer = inputData.ContainsKey("Manufacturer") ? inputData["Manufacturer"] : null;
//                sellableItem.TypeOfGood = inputData.ContainsKey("TypeOfGood") ? inputData["TypeOfGood"] : null;
//                sellableItem.Tags = new List<Tag>();
//                if (inputData.ContainsKey("SamClass"))
//                {
//                    sellableItem.Tags.Add(new Tag(inputData["SamClass"]));
//                }
//                workflowComponent = sellableItem.GetComponent<WorkflowComponent>();
//                if (workflowComponent == null)
//                {
//                    workflowComponent = new WorkflowComponent();
//                }

//                workflowComponent.Workflow = new EntityReference { EntityTarget = "Entity-Workflow-UVSCustomWorkflow", Name = "UVSCustomWorkflow" };
//                workflowComponent.CurrentState = "Updated";

//                sellableItem.SetComponent(workflowComponent);
//                sellableItemResponse.SellableItem = sellableItem;
//                sellableItemResponse.StatusCode = 200;
//                sellableItemResponse.IsNew = false;
//            }
//            return sellableItemResponse;
//        }
//    }
//}