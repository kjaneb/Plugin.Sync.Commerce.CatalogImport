
using System.Collections.Generic;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.Core.Commands;
using Sitecore.Framework.Conditions;
using Sitecore.Framework.Pipelines;
using System.Threading.Tasks;
using Microsoft.AspNet.OData.Builder;
using Plugin.Sync.Commerce.CatalogImport.Models;
using Sitecore.Framework.Pipelines.Abstractions;

namespace Plugin.Sync.Commerce.CatalogImport
{
    /// <summary>
    /// Defines a block which configures the OData model
    /// </summary>
    /// <seealso>
    ///     <cref>
    ///         Sitecore.Framework.Pipelines.PipelineBlock{Microsoft.AspNetCore.OData.Builder.ODataConventionModelBuilder,
    ///         Microsoft.AspNetCore.OData.Builder.ODataConventionModelBuilder,
    ///         Sitecore.Commerce.Core.CommercePipelineExecutionContext}
    ///     </cref>
    /// </seealso>
    [PipelineDisplayName("CatalogSyncConfigureServiceApiBlock")]
    public class ConfigureServiceApiBlock : AsyncPipelineBlock<ODataConventionModelBuilder, ODataConventionModelBuilder, CommercePipelineExecutionContext>
    {
        public override Task<ODataConventionModelBuilder> RunAsync(ODataConventionModelBuilder modelBuilder, CommercePipelineExecutionContext context)
        {
            Condition.Requires(modelBuilder).IsNotNull($"{this.Name}: The argument cannot be null.");

            var importCategory = modelBuilder.Action("ImportCategory");
            importCategory.ReturnsFromEntitySet<CommerceCommand>("Commands");

            var importSellableItem = modelBuilder.Action("ImportSellableItem");
            importSellableItem.ReturnsFromEntitySet<CommerceCommand>("Commands");

            var importSellableItemVariant = modelBuilder.Action("ImportVariant");
            importSellableItemVariant.ReturnsFromEntitySet<CommerceCommand>("Commands");

            var importItemDefintion = modelBuilder.Action("ImportItemDefinition");
            importItemDefintion.Parameter<string>("name");
            importItemDefintion.Parameter<string>("displayName");
            importItemDefintion.Parameter<ItemDefinitionProperties>("properties");
            importItemDefintion.ReturnsFromEntitySet<CommerceCommand>("Commands");

            var importSellableItemFromContentHub = modelBuilder.Action("ImportSellableItemFromContentHub");
            importSellableItemFromContentHub.ReturnsFromEntitySet<CommerceCommand>("Commands");

            var processAzureQueue = modelBuilder.Action("ProcessAzureQueue");
            processAzureQueue.ReturnsFromEntitySet<CommerceCommand>("Commands");

            return Task.FromResult(modelBuilder);
        }

    }
}
