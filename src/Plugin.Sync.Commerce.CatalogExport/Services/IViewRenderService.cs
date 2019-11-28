using System.Threading.Tasks;

namespace Plugin.Sync.Commerce.CatalogExport.Services
{
    public interface IViewRenderService
    {
        Task<string> RenderAsync(string name);

        Task<string> RenderAsync<TModel>(string name, TModel model);
    }
}