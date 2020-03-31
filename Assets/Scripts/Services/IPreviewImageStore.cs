using System.Threading.Tasks;
using StlVault.Config;

namespace StlVault.Services
{
    internal interface IPreviewImageStore
    {
        Task<byte[]> LoadPreviewAsync(string fileHash);
        Task StorePreviewAsync(string fileHash, byte[] imageData);
    }
}