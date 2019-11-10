using Sitecore.Commerce.Core;

namespace Plugin.Sync.Commerce.CatalogImport.Pipelines.Arguments
{
    public class ProductImportMinionArgument : PipelineArgument
    {
        public ProductImportMinionArgument(int batchSize, int maxFetchSize)
        {
            this.BatchSize = batchSize;
            this.MaxFetchSize = maxFetchSize;
        }
        public int BatchSize { get; set; }
        public int MaxFetchSize { get; set; }
    }
}