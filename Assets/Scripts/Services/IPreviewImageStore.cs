using System;
using System.Threading.Tasks;

namespace StlVault.Services
{
    internal interface IPreviewImageStore
    {
        Task<byte[]> LoadPreviewAsync(string fileHash);
        Task StorePreviewAsync(string fileHash, byte[] imageData);
    }
}