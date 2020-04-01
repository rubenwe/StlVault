using System.Collections.Generic;
using System.Threading.Tasks;
using StlVault.Config;

namespace StlVault.Services
{
    internal interface IMetaDataStore
    {
        Task<IReadOnlyList<TagInfo>> GetTagsForItemAsync(string fileHash);
        Task<string> GetMainHashForItemAsync(string fileHash);
        Task StoreTagsForItemAsync(string fileHash, IReadOnlyList<string> tags);
        Task StoreAlternativeHash(string fileHash, string altHash);
    }
}