using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using StlVault.Config;
using StlVault.Util.FileSystem;
using static StlVault.Constants;

namespace StlVault.AppModel
{
    internal sealed class ImportFolder
    {
        [NotNull] private readonly ILibrary _library;
        [NotNull] private readonly IKnownItemStore _store;
        [NotNull] private readonly IFileSystem _fileSystem;
        [NotNull] private readonly ImportFolderConfig _config;
        
        public ImportFolder(
            [NotNull] ImportFolderConfig config,
            [NotNull] IFileSystem fileSystem,
            [NotNull] IKnownItemStore store,
            [NotNull] ILibrary library)
        {
            _store = store ?? throw new ArgumentNullException(nameof(store));
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _library = library ?? throw new ArgumentNullException(nameof(library));
            _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
        }
        
        private string GetFullPath(string relativePath) => Path.Combine(_config.FullPath, relativePath);
        private string GetFullPath(IFileInfo item) => GetFullPath(item.RelativePath);
        
        public Task InitializeAsync()
        {
            return Task.Run(async () =>
            {
                var files = await _store.GetKnownItemsInLocationAsync(_config.FullPath, _config.ScanSubDirectories) 
                            ?? new List<KnownItemInfo>();
                
                var knownFiles = files.ToDictionary(item => item.RelativePath);

                var matchedFiles = _fileSystem
                    .GetFiles(SupportedFilePattern, _config.ScanSubDirectories)
                    .ToDictionary(item => item.RelativePath);
                
                var importFiles = new HashSet<string>();
                var removeFiles = new HashSet<string>();
                foreach (var (filePath, fileInfo) in matchedFiles)
                {
                    // New files
                    if (!knownFiles.ContainsKey(filePath))
                    {
                        importFiles.Add(GetFullPath(filePath));
                        knownFiles[filePath] = new KnownItemInfo(fileInfo);
                    }
                    // Files that have changed
                    else if (knownFiles.TryGetValue(filePath, out var known) &&
                             known.LastChange != fileInfo.LastChange)
                    {
                        removeFiles.Add(GetFullPath(known));
                        importFiles.Add(GetFullPath(filePath));
                        knownFiles[filePath] = new KnownItemInfo(fileInfo);
                    }
                }

                // Missing files
                var missingFiles = knownFiles.Keys.Where(filePath => !matchedFiles.ContainsKey(filePath)).ToList();
                foreach (var filePath in missingFiles)
                {
                    removeFiles.Add(filePath);
                    knownFiles.Remove(filePath);
                }
                
                await _library.RemoveRangeAsync(_config, removeFiles);
                await _library.ImportRangeAsync(_config, importFiles);
                await _store.SaveKnownItemsForLocationAsync(_config.FullPath, knownFiles.Values.ToList());
                
                var watcher = _fileSystem.CreateWatcher(SupportedFilePattern, _config.ScanSubDirectories);
                watcher.FileAdded += WatcherOnFileAdded;
                watcher.FileRemoved += WatcherOnFileRemoved;
            });
        }

        private async void WatcherOnFileRemoved(object sender, string e)
        {
            throw new NotImplementedException();
        }

        private async void WatcherOnFileAdded(object sender, string e)
        {
            throw new NotImplementedException();
        }
    }
}