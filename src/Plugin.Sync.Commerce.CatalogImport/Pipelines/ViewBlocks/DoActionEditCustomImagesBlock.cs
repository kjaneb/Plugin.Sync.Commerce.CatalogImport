using Newtonsoft.Json;
using Plugin.Sync.Commerce.CatalogImport.Helpers;
using Serilog;
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
    public class DoActionEditCustomImagesBlock : PipelineBlock<EntityView, EntityView, CommercePipelineExecutionContext>
    {
        private readonly CommerceCommander _commerceCommander;

        public DoActionEditCustomImagesBlock(CommerceCommander commerceCommander)
        {
            _commerceCommander = commerceCommander;
        }

        public override Task<EntityView> Run(EntityView arg, CommercePipelineExecutionContext context)
        {
            Condition.Requires(arg).IsNotNull($"{Name}: The argument cannot be null.");
            
            // Only proceed if the right action was invoked
            if (string.IsNullOrEmpty(arg.Action) || !arg.Action.Equals(Constant.KnownCustomImagesView.EditCustomImages, StringComparison.OrdinalIgnoreCase))
            {
                return Task.FromResult(arg);
            }

            // Get the sellable item from the context
            var entity = context.CommerceContext.GetObject<SellableItem>(x => x.Id.Equals(arg.EntityId));
            if (entity == null)
            {
                return Task.FromResult(arg);
            }

            // Get the notes component from the sellable item or its variation
            //var component = entity.GetComponent<CustomImagesComponent>(arg.ItemId);
            //if (component == null)
            //{
            //    component = new CustomImagesComponent();
            //}

            //var rawValue = arg.Properties.FirstOrDefault(x => x.Name.Equals("Images", StringComparison.OrdinalIgnoreCase))?.Value;
            //if (!string.IsNullOrEmpty(rawValue))
            //{
            //    try
            //    {
            //        component.Images = JsonConvert.DeserializeObject<Dictionary<string, string>>(rawValue);
            //    }
            //    catch (Exception ex)
            //    {
            //        Log.Error(ex, $"Error deserializing images collection. Item ID: {arg?.ItemId}");
            //        //TODO: 
            //    }
            //}

            //new Dictionary<string, string>();

            //TODO - Optimize this code to show one image record at a time.
            //for (int i = 1; i <= arg.Properties.Count / 2; i++)
            //{
            //    string key = arg.Properties.FirstOrDefault(x => x.Name.Equals($"Key{i}", StringComparison.OrdinalIgnoreCase))?.Value;
            //    string value = arg.Properties.FirstOrDefault(x => x.Name.Equals($"Url{i}", StringComparison.OrdinalIgnoreCase))?.Value;
            //    component.Images.Add(key, value);
            //}

            //component.ImagesJson = JsonConvert.SerializeObject(component.Images);
            // Persist changes
            this._commerceCommander.Pipeline<IPersistEntityPipeline>().Run(new PersistEntityArgument(entity), context);

            return Task.FromResult(arg);
        }
    }
}
