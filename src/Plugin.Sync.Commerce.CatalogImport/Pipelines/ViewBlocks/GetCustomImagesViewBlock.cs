using Newtonsoft.Json;
using Plugin.Sync.Commerce.CatalogImport.Helpers;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.EntityViews;
using Sitecore.Commerce.Plugin.Catalog;
using Sitecore.Framework.Conditions;
using Sitecore.Framework.Pipelines;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plugin.Sync.Commerce.CatalogImport.Pipelines.ViewBlocks
{
    public class GetCustomImagesViewBlock : PipelineBlock<EntityView, EntityView, CommercePipelineExecutionContext>
    {
        private readonly ViewCommander _viewCommander;

        public GetCustomImagesViewBlock(ViewCommander viewCommander)
        {
            _viewCommander = viewCommander;
        }

        public override Task<EntityView> Run(EntityView arg, CommercePipelineExecutionContext context)
        {
            Condition.Requires(arg).IsNotNull($"{Name}: The argument cannot be null.");
            var request = _viewCommander.CurrentEntityViewArgument(context.CommerceContext);

            var catalogViewsPolicy = context.GetPolicy<KnownCatalogViewsPolicy>();

            var isVariationView = request.ViewName.Equals(catalogViewsPolicy.Variant, StringComparison.OrdinalIgnoreCase);
            var isConnectView = arg.Name.Equals(catalogViewsPolicy.ConnectSellableItem, StringComparison.OrdinalIgnoreCase);

            // Only proceed if the current entity is a sellable item
            if (!(request.Entity is SellableItem))
            {
                return Task.FromResult(arg);
            }

            // See if we are dealing with the base sellable item or one of its variations.
            var variationId = string.Empty;
            if (isVariationView && !string.IsNullOrEmpty(arg.ItemId))
            {
                variationId = arg.ItemId;
            }

            var targetView = arg;

            // Check if the edit action was requested
            var isEditView = !string.IsNullOrEmpty(arg.Action) && (arg.Action.Equals(Constant.KnownCustomImagesView.EditCustomImages, StringComparison.OrdinalIgnoreCase));
            if (!isEditView)
            {
                var view = new EntityView
                {
                    Name = Constant.KnownCustomImagesView.ViewName,
                    DisplayName = "Custom Images",
                    EntityId = arg.EntityId,
                    ItemId = variationId
                };

                arg.ChildViews.Add(view);
                targetView = view;
            }

            //TODO - Show custom images in grid (tabular) format.
            var sellableItem = (SellableItem)request.Entity;
            //if (sellableItem != null && (sellableItem.HasComponent<CustomImagesComponent>(variationId) || isConnectView || isEditView))
            //{
            //    var component = sellableItem.GetComponent<CustomImagesComponent>();

            //    //if (component != null && component.Images != null && component.Images.Count > 0)
            //    //{
            //    //    targetView.Properties.Add(
            //    //        new ViewProperty
            //    //        {
            //    //            Name = "Images",
            //    //            DisplayName = "Images",
            //    //            RawValue = JsonConvert.SerializeObject(component.Images),
            //    //            IsReadOnly = !isEditView,
            //    //            IsRequired = true,
            //    //            UiType = "ItemLink"
            //    //        });
            //    //}

            //    if (component != null && component.Images != null && component.Images.Count > 0)
            //    {
            //        targetView.Properties.Add(
            //            new ViewProperty
            //            {
            //                Name = "ImagesJson",
            //                DisplayName = "Images JSON",
            //                RawValue = JsonConvert.SerializeObject(component.Images),
            //                IsReadOnly = !isEditView,
            //                IsRequired = false,
            //                UiType = "ItemLink"
            //            });
            //    }

            //    if (!isConnectView)
            //    {
            //        var index = 1;
            //        foreach (var image in component.Images)
            //        {
            //            targetView.Properties.Add(
            //                   new ViewProperty
            //                   {
            //                       Name = "Key" + index.ToString(),
            //                       DisplayName = "Image Name",
            //                       RawValue = image.Key,
            //                       IsReadOnly = !isEditView,
            //                       IsRequired = true,
            //                       UiType = "ItemLink"
            //                   });

            //            targetView.Properties.Add(
            //                new ViewProperty
            //                {
            //                    Name = "Url" + index.ToString(),
            //                    DisplayName = "Image Url",
            //                    RawValue = image.Value,
            //                    IsReadOnly = !isEditView,
            //                    IsRequired = true,
            //                    UiType = "ItemLink"
            //                });

            //            index = index + 1;
            //        } 
            //    }
            //}
            return Task.FromResult(arg);
        }
    }
}
