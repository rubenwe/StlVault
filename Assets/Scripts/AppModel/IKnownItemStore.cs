using System.Collections.Generic;
using System.Threading.Tasks;

namespace StlVault.AppModel
{
    internal interface IKnownItemStore
    {
        Task<IReadOnlyList<KnownItemInfo>> GetKnownItemsInLocationAsync(string rootPath, bool recursive);
        Task SaveKnownItemsForLocationAsync(string rootPath, IReadOnlyList<KnownItemInfo> knownFiles);
    }
}