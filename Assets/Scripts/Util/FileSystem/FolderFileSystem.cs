using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using StlVault.Util.Logging;

namespace StlVault.Util.FileSystem
{
    internal class FolderFileSystem : IFileSystem
    {
        private static readonly ILogger Logger = UnityLogger.Instance;
        
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

        public IReadOnlyList<IFileInfo> GetFiles(string pattern, bool recursive)
        {
            var options = recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
            var files = new List<IFileInfo>();
            
            foreach (var file in Directory.GetFiles(_rootPath, pattern, options))
            {
                try
                {
                    var info = new System.IO.FileInfo(file);

                    files.Add(new FileInfo
                    {
                        LastChange = Max(info.CreationTime, info.LastWriteTime),
                        Path = file.Substring(_rootPath.Length).Trim(Path.DirectorySeparatorChar),
                        Size = info.Length
                    });
                }
                catch (Exception ex)
                {
                    Logger.Warn(ex, "Error while getting FileInfos for {0}", file);
                }
            }

            return files;
        }

        public Task<byte[]> ReadAllBytesAsync(string filePath)
        {
            var path = Path.Combine(_rootPath, filePath);
            return Task.Run(() => File.ReadAllBytes(path));
        }

        private static DateTime Max(DateTime first, DateTime second) => first > second ? first : second;

        private class FileInfo : IFileInfo
        {
            public string Path { get; set; }
            public DateTime LastChange { get; set; }
            public long Size { get; set; }
        }
    }
}