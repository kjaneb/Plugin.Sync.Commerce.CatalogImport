using Microsoft.AspNetCore.Hosting;
using Plugin.Sync.Commerce.CatalogExport.Util;
using Plugin.Sync.Commerce.CatalogExport.Pipelines.Arguments;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.Plugin.Management;
using Sitecore.Framework.Pipelines;
using Sitecore.Services.Core.Model;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Plugin.Sync.Commerce.CatalogExport.Pipelines.Blocks
{

    /// <summary>
    /// Gets Sitecore content item by path and extracts field value (by name)
    /// </summary>
    public class GetTemplateFromFilesystemBlock : PipelineBlock<ExportCommerceEntityArgument, ExportCommerceEntityArgument, CommercePipelineExecutionContext>
    {
        IHostingEnvironment _hostingEnvironment;

        public GetTemplateFromFilesystemBlock(IHostingEnvironment hostingEnvironment)
        {
            _hostingEnvironment = hostingEnvironment;
        }

#pragma warning disable 1998
        public override async Task<ExportCommerceEntityArgument> Run(ExportCommerceEntityArgument arg, CommercePipelineExecutionContext context)
        {
            if (!arg.TemplateLocation.Equals("file", StringComparison.OrdinalIgnoreCase))
                return arg;

            var argumentPath = arg.TemplatePath.Replace("/", "\\");
            string filePath = null;
            if (File.Exists(argumentPath))
                filePath = argumentPath;
            else if (File.Exists(Path.Combine(_hostingEnvironment.WebRootPath, argumentPath)))
                filePath = Path.Combine(_hostingEnvironment.WebRootPath, argumentPath);

            if (filePath == null)
            {
                arg.EntityNotFound = true;
                return context.AbortPipeline(arg, $"Template file not found. Searched locations: '{arg.TemplatePath}', '{Path.Combine(_hostingEnvironment.WebRootPath, arg.TemplatePath)}' {this.GetType().Name}. Request EntityId={arg.EntityId}.");
            }

            try
            {
                var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                using (var streamReader = new StreamReader(fileStream, Encoding.UTF8))
                {
                    arg.ViewTemplate = streamReader.ReadToEnd();
                    if (string.IsNullOrEmpty(arg.ViewTemplate))
                    {
                        return context.AbortPipeline(arg, $"Template file was found, but appear empty. File path: '{filePath}', EntityId: {arg.EntityId}");
                    }

                    return arg;
                }
            }
            catch (Exception ex)
            {
                arg.EntityNotFound = true;
                return context.AbortPipeline(arg, $"Eror reading template contents. File path: '{filePath}'. Error: {ex.Message}");
                throw ex;
            }
        }
#pragma warning restore 1998
    }
}
