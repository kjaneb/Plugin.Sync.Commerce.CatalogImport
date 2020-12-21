using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Plugin.Sync.Commerce.CatalogImport.Extensions;
using Plugin.Sync.Commerce.CatalogImport.Models;
using Plugin.Sync.Commerce.CatalogImport.Pipelines.Arguments;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.Core.Commands;
using Sitecore.Commerce.EntityViews;
using Sitecore.Commerce.Plugin.Catalog;
using Sitecore.Commerce.Plugin.Composer;
using Sitecore.Commerce.Plugin.ManagedLists;
using Sitecore.Commerce.Plugin.Pricing;
using Sitecore.Commerce.Plugin.Views;
using Sitecore.Framework.Conditions;
using Sitecore.Framework.Pipelines;

namespace Plugin.Sync.Commerce.CatalogImport.Pipelines.Blocks
{
    public class CreateOrUpdateComposerTemplateBlock : AsyncPipelineBlock<ImportItemDefinitionArgument, ImportItemDefinitionArgument, CommercePipelineExecutionContext>
    {
        private readonly CommerceCommander _commerceCommander;
        private readonly CommerceEntityImportHelper _importHelper;

        public CreateOrUpdateComposerTemplateBlock(CommerceCommander commerceCommander, ComposerCommander composerCommander)
        {
            _commerceCommander = commerceCommander;
        }

        public override async Task<ImportItemDefinitionArgument> RunAsync(ImportItemDefinitionArgument arg, CommercePipelineExecutionContext context)
        {
            var idName = CommerceEntity.IdPrefix<ComposerTemplate>() + arg.Name;

            var template = await _commerceCommander.Command<FindEntityCommand>().Process(context.CommerceContext, typeof(ComposerTemplate), idName) as ComposerTemplate;

            if (template == null)
            {
                template = new ComposerTemplate(idName)
                {
                    Name = arg.Name,
                    DisplayName = arg.DisplayName
                };

                template.SetComponent(new ListMembershipsComponent
                    {
                        Memberships =
                        {
                            string.Format("{0}", CommerceEntity.ListName<ComposerTemplate>())
                        }
                    }
                );
            }

            var templateEntityViewComponent = template.GetComponent<EntityViewComponent>();

            if (templateEntityViewComponent == null)
            {
                templateEntityViewComponent = new EntityViewComponent();
                template.SetComponent(templateEntityViewComponent);
            }

            var entityView = templateEntityViewComponent.View.GetChildViews(x => x.Name == "Specifications").FirstOrDefault();

            if (entityView == null)
            {
                entityView = new EntityView
                {
                    Name = "Specifications"
                };

                entityView.SetItemIdForComposerView();

                templateEntityViewComponent.AddChildView(entityView);
            }

            foreach (var field in arg.Fields)
            {
                if (!entityView.ContainsProperty(field))
                {
                    var property = new ViewProperty
                    {
                        Name = field,
                        OriginalType = "System.String",
                        IsRequired = false
                    };

                    entityView.Properties.Add(property);
                }
            }

            var result = await _commerceCommander.Pipeline<IPersistEntityPipeline>().RunAsync(new PersistEntityArgument(template), context);

            return arg;
        }
    }
}
