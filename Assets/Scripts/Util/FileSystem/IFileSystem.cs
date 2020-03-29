using System.Collections.Generic;
using JetBrains.Annotations;

namespace StlVault.Util.FileSystem
{
    internal interface IFileSystem
    {
        [NotNull]
        IFolderWatcher CreateWatcher(string filter, bool scanSubDirectories);

        bool FileExists(string filePath);
        IEnumerable<IFileInfo> GetFiles(string pattern, bool recursive);
    }
}