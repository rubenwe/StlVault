using System;

namespace StlVault.Util.FileSystem
{
    internal interface IFileInfo
    {
        string RelativePath { get; }
        DateTime LastChange { get; }
    }
}