using System.Threading.Tasks;

namespace StlVault.AppModel
{
    internal interface IConfigStore
    {
        Task<T> TryLoadAsync<T>() where T : class, new();
        Task StoreAsync<T>(T config);
    }
}