using System.Collections.Generic;
using System.Threading.Tasks;
using StlVault.AppModel;

namespace StlVault.Views
{
    internal class AppDataKnownItemStore : IKnownItemStore
    {
        public Task<IReadOnlyList<KnownItemInfo>> GetKnownItemsInLocationAsync(string rootPath, bool recursive)
        {
            return Task.FromResult<IReadOnlyList<KnownItemInfo>>(new List<KnownItemInfo>());
        }

        public Task SaveKnownItemsForLocationAsync(string rootPath, IReadOnlyList<KnownItemInfo> knownFiles)
        {
            return Task.CompletedTask;
        }
    }
}