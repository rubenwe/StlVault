using System.Threading.Tasks;
using StlVault.Config;

namespace StlVault.Services
{
    internal interface IPreviewImageStore
    {
        Task<byte[]> GetPreviewImageForItemAsync(string fileHash, ConfigVector3 rotation);
        Task StorePreviewImageForItemAsync(string fileHash, byte[] imageData, ConfigVector3 rotation);
    }
}