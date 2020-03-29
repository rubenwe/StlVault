using System;
using System.IO;
using StlVault.Util.FileSystem;

namespace StlVault.Util
{
    internal sealed class FolderWatcher : IFolderWatcher, IDisposable
    {
        private readonly FileSystemWatcher _watcher;

        public event EventHandler<string> FileAdded;
        public event EventHandler<string> FileRemoved;

        public FolderWatcher(string path, string filter, bool includeSubdirectories = true)
        {
            _watcher = new FileSystemWatcher
            {
                Path = path,
                Filter = filter,
                IncludeSubdirectories = includeSubdirectories,
                NotifyFilter = NotifyFilters.LastWrite
                               | NotifyFilters.CreationTime
                               | NotifyFilters.Size
                               | NotifyFilters.FileName
            };

            _watcher.Changed += OnFileSystemEvent;
            _watcher.Created += OnFileSystemEvent;
            _watcher.Renamed += OnFileSystemEvent;
            _watcher.Deleted += OnFileSystemEvent;

            _watcher.EnableRaisingEvents = true;
        }

        private void OnFileSystemEvent(object sender, FileSystemEventArgs e)
        {
            // ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
            switch (e.ChangeType)
            {
                case WatcherChangeTypes.Created:
                    FileAdded?.Invoke(this, e.FullPath);
                    break;

                case WatcherChangeTypes.Deleted:
                    FileRemoved?.Invoke(this, e.FullPath);
                    break;

                case WatcherChangeTypes.Changed:
                    FileRemoved?.Invoke(this, e.FullPath);
                    if (File.Exists(e.FullPath))
                    {
                        FileAdded?.Invoke(this, e.FullPath);
                    }

                    break;
            }
        }

        public void Dispose()
        {
            _watcher?.Dispose();
        }
    }
}