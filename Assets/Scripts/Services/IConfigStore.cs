using System.Threading.Tasks;

namespace StlVault.Services
{
    internal interface IConfigStore
    {
        Task<T> LoadAsyncOrDefault<T>() where T : class, new();
        T LoadOrDefault<T>() where T : class, new();
        Task StoreAsync<T>(T config);
    }
}