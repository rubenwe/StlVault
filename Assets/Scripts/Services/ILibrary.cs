using System.Collections.Generic;
using System.Threading.Tasks;
using StlVault.Config;

namespace StlVault.Services
{
    internal interface ILibrary
    {
        IReadOnlyList<ItemPreviewMetadata> GetItemPreviewMetadata(IReadOnlyList<string> filters);
        Task RemoveRangeAsync(ImportFolderConfig folderConfig, IReadOnlyCollection<string> filesToRemove);
        Task ImportRangeAsync(ImportFolderConfig folderConfig, IReadOnlyCollection<string> filesToImport);
    }
}