using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using JetBrains.Annotations;
using StlVault.Config;
using StlVault.Util;
using StlVault.Util.FileSystem;
using static StlVault.Constants;
using static StlVault.Services.FolderState;
using Timer = System.Timers.Timer;

namespace StlVault.Services
{
    internal sealed class ImportFolder : IImportFolder
    {
        [NotNull] private readonly Timer _timer;
        [NotNull] private readonly ILibrary _library;
        [NotNull] private readonly IKnownItemStore _store;
        [NotNull] private readonly IFileSystem _fileSystem;
        [CanBeNull] private IFolderWatcher _watcher;
        
        [NotNull] public BindableProperty<FolderState> FolderState { get; } = new BindableProperty<FolderState>(Ok);
        [NotNull] public ImportFolderConfig Config { get; }

        public ImportFolder(
            [NotNull] ImportFolderConfig config,
            [NotNull] IFileSystem fileSystem,
            [NotNull] IKnownItemStore store,
            [NotNull] ILibrary library)
        {
            _store = store ?? throw new ArgumentNullException(nameof(store));
            Config = config ?? throw new ArgumentNullException(nameof(config));
            _library = library ?? throw new ArgumentNullException(nameof(library));
            _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));

            _timer = new Timer(500) {AutoReset = false, Enabled = false};
            _timer.Elapsed += TimerOnElapsed;
        }

        private string GetFullPath(string relativePath) => Path.Combine(Config.FullPath, relativePath);
        private string GetFullPath(IFileInfo item) => GetFullPath(item.RelativePath);

        public Task InitializeAsync()
        {
            return Task.Run(RescanItems);
        }

        private volatile bool _rescanRunning;
        private async Task RescanItems()
        {
            if (_rescanRunning) return;
            _rescanRunning = true;
            
            FolderState.Value = Refreshing;

            var files = await _store.GetKnownItemsInLocationAsync(Config.FullPath, Config.ScanSubDirectories) ?? new List<KnownItemInfo>();
            var knownFiles = files.ToDictionary(item => item.RelativePath);

            var matchedFiles = _fileSystem.GetFiles(SupportedFilePattern, Config.ScanSubDirectories)
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
                else if (knownFiles.TryGetValue(filePath, out var known) && known.LastChange != fileInfo.LastChange)
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

            await _library.RemoveRangeAsync(Config, removeFiles);
            await _library.ImportRangeAsync(Config, importFiles);
            await _store.SaveKnownItemsForLocationAsync(Config.FullPath, knownFiles.Values.ToList());

            InitializeWatcher();

            FolderState.Value = Ok;
            _rescanRunning = false;
        }

        private void InitializeWatcher()
        {
            if (_watcher == null)
            {
                _watcher = _fileSystem.CreateWatcher(SupportedFilePattern, Config.ScanSubDirectories);
                _watcher.FileAdded += WatcherOnFileAdded;
                _watcher.FileRemoved += WatcherOnFileRemoved;
            }
        }

        private void WatcherOnFileRemoved(object sender, string e) => ResetTimer();
        private void WatcherOnFileAdded(object sender, string e) => ResetTimer();

        private async void TimerOnElapsed(object sender, ElapsedEventArgs e)
        {
            while (_rescanRunning)
            {
                await Task.Delay(500);
            }
            
            await Task.Run(RescanItems);
        }
        
        private void ResetTimer()
        {
            lock (_timer)
            {
                _timer.Stop();
                _timer.Start();
            }
        }
 
        public void Dispose() => _watcher?.Dispose();
    }
}