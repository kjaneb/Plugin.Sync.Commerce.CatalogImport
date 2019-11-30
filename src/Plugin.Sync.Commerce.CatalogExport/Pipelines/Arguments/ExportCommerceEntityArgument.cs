using Sitecore.Commerce.Core;

namespace Plugin.Sync.Commerce.CatalogExport.Pipelines.Arguments
{
    /// <summary>
    /// Argument for ExportCommerceEntityPipeline and pipeline blocks in it 
    /// </summary>
    public class ExportCommerceEntityArgument : PipelineArgument
    {
        public ExportCommerceEntityArgument(string entitId, string templatePath, string templateLocation)
        {
            this.EntityId = entitId;
            this.TemplatePath = templatePath;
            this.TemplateLocation = templateLocation;
        }
        public string EntityId { get; set; }
        public bool Success { get; set; } = true;
        public bool EntityNotFound { get; set; }
        public string TemplatePath { get; set; }
        public string TemplateLocation { get; set; }
        public string Response { get; set; }
        public string ErrorMessage { get; set; }
        public string ViewTemplate { get; set; }
    }
}
