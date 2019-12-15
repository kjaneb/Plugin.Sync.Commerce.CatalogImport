using Sitecore.Commerce.Core;

namespace Plugin.Sync.Commerce.EntitiesMigration.Pipelines.Arguments
{
    /// <summary>
    /// ImportComposerTemplatesArgument
    /// </summary>
    public class ImportEntitiesArgument : PipelineArgument
    {
        /// <summary>
        /// Import Type
        /// </summary>
        public ImportType ImportType { get; set; }
        public string InputJson { get; set; }

        /// <summary>
        /// c'tor
        /// </summary>
        public ImportEntitiesArgument()
        {
            ImportType = ImportType.Skip;
        }
    }
}