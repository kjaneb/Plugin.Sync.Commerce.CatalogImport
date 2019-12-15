using Sitecore.Commerce.Core;

namespace Plugin.Sync.Commerce.EntitiesMigration.Pipelines.Arguments
{
    /// <summary>
    /// ImportComposerTemplatesArgument
    /// </summary>
    public class ImportEntitiesArgument : PipelineArgument
    {
        /// <summary>s
        /// Import Type
        /// </summary>
        public string EntityType { get; set; }
        public string InputJson { get; set; }

        /// <summary>
        /// c'tor
        /// </summary>s
        public ImportEntitiesArgument()
        {
            //ImportType = ImportType.Skip;
        }
    }
}