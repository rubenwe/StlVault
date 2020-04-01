using System;

namespace StlVault.Util.FileSystem
{
    internal interface IFileInfo
    {
        string Path { get; }
        DateTime LastChange { get; }
    }

    internal static class FileInfoExtensions
    {
        public static void Deconstruct(this IFileInfo fileInfo, out string path, out DateTime lastChange)
        {
            path = fileInfo.Path;
            lastChange = fileInfo.LastChange;
        }
    }
}