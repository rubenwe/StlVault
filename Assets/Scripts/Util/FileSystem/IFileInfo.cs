using System;

namespace StlVault.Util.FileSystem
{
    internal interface IFileInfo
    {
        string Path { get; }
        DateTime LastChange { get; }
        long Size { get; }
    }
}