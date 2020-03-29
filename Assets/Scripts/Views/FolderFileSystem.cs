using System;
using System.Collections.Generic;
using System.IO;
using StlVault.Util;
using StlVault.Util.FileSystem;

namespace StlVault.Views
{
    internal class FolderFileSystem : IFileSystem
    {
        private readonly string _rootPath;

        public FolderFileSystem(string rootPath)
        {
            _rootPath = rootPath;
        }

        public IFolderWatcher CreateWatcher(string filter, bool scanSubDirectories)
        {
            return new FolderWatcher(_rootPath, filter, scanSubDirectories);
        }

        public bool FileExists(string filePath) => File.Exists(Path.Combine(_rootPath, filePath));

        public IEnumerable<IFileInfo> GetFiles(string pattern, bool recursive)
        {
            var options = recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
            foreach (var file in Directory.GetFiles(_rootPath, pattern, options))
            {
                var info = new System.IO.FileInfo(file);

                yield return new FileInfo
                {
                    LastChange = Max(info.CreationTime, info.LastWriteTime),
                    RelativePath = file.Substring(_rootPath.Length).Trim(Path.DirectorySeparatorChar)
                };
            }
        }

        private static DateTime Max(DateTime first, DateTime second) => first > second ? first : second;

        private class FileInfo : IFileInfo
        {
            public string RelativePath { get; set; }
            public DateTime LastChange { get; set; }
        }
    }
}