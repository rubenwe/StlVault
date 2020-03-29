using System;
using System.Collections.Generic;
using System.IO;

namespace StlVault.Util.FileSystem
{
    internal interface IFileSystem
    {
        IFolderWatcher CreateWatcher(string filter, bool scanSubDirectories);
        bool FileExists(string filePath);
        IEnumerable<IFileInfo> GetFiles(string pattern, bool recursive);
    }

    internal interface IFileInfo
    {
        string RelativePath { get; }
        DateTime LastChange { get; }
    }
}