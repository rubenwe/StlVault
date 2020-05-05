using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace StlVault.Util.FileSystem
{
    internal interface IFileSystem
    {
        [NotNull]
        IFolderWatcher CreateWatcher(string filter, bool scanSubDirectories);

        bool FileExists(string filePath);
        IReadOnlyList<IFileInfo> GetFiles(string pattern, bool recursive);
        Task<byte[]> ReadAllBytesAsync(string filePath);
    }
}