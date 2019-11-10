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
    public class PopulateCustomImagesActionBlock : PipelineBlock<EntityView, EntityView, CommercePipelineExecutionContext>
    {
        public PopulateCustomImagesActionBlock(CommerceCommander commerceCommander)
        {
        }

        public override Task<EntityView> Run(EntityView arg, CommercePipelineExecutionContext context)
        {
            Condition.Requires(arg).IsNotNull($"{Name}: The argument cannot be null.");
            
            if (string.IsNullOrEmpty(arg?.Name) || !arg.Name.Equals(Constant.KnownCustomImagesView.ViewName, StringComparison.OrdinalIgnoreCase))
            {
                return Task.FromResult(arg);
            }

            var actionPolicy = arg.GetPolicy<ActionsPolicy>();

            actionPolicy.Actions.Add(
              new EntityActionView
              {
                  Name = Constant.KnownCustomImagesView.EditCustomImages,
                  DisplayName = "Edit Sellable Item Custom Images",
                  Description = "Edit the sellable item custom images",
                  IsEnabled = true,
                  EntityView = arg.Name,
                  Icon = "edit"
              });

            return Task.FromResult(arg);
        }
    }
}
