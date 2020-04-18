using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using StlVault.Config;
using StlVault.Util;
using StlVault.Util.FileSystem;
using StlVault.Util.Logging;
using static StlVault.Constants;
using static StlVault.Services.FileSourceState;

namespace StlVault.Services
{
    [DebuggerDisplay("{" + nameof(DisplayName) + "}")]
    internal sealed class ImportFolder : FileSourceBase, IImportFolder
    {
        private readonly Dictionary<string, IFileInfo> _knownFiles = new Dictionary<string, IFileInfo>();
        
        // [CanBeNull] private IFolderWatcher _watcher;
        // [NotNull] private readonly Timer _timer;
        [NotNull] private readonly IFileSystem _fileSystem;
        [NotNull] private readonly ImportFolderConfig _config;
        [NotNull] public override FileSourceConfig Config => _config;
        [NotNull] public override string DisplayName => _config.FullPath;

        private CancellationTokenSource _tokenSource;
        
        public ImportFolder(
            [NotNull] ImportFolderConfig config,
            [NotNull] IFileSystem fileSystem)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));

            // _timer = new Timer(500) {AutoReset = false, Enabled = false};
            // _timer.Elapsed += TimerOnElapsed;
        }

        public override async Task InitializeAsync()
        {
            await RescanItemsAsync();
            // await InitializeWatcherAsync();
        }

        public override Task<byte[]> GetFileBytesAsync(string resourcePath)
        {
            return _fileSystem.ReadAllBytesAsync(resourcePath);
        }

        public override IReadOnlyCollection<string> GetTags(string resourcePath)
        {
            return ImportFolderTagHelper.GenerateTags(_config, resourcePath);
        }

        private volatile bool _rescanRunning;
        
        private async Task RescanItemsAsync()
        {
            if (_rescanRunning) return;
            _rescanRunning = true;
            
            _tokenSource = new CancellationTokenSource();

            State.Value = Refreshing;
            
            try
            {
                var importFiles = new HashSet<IFileInfo>();
                var removeFiles = new HashSet<string>();
                
                await Task.Run(() =>
                {
                    var matchedFiles = _fileSystem
                        .GetFiles(SupportedFilePattern, _config.ScanSubDirectories)
                        .ToDictionary(item => item.Path);

                    foreach (var (filePath, fileInfo) in matchedFiles)
                    {
                        // New files
                        if (!_knownFiles.ContainsKey(filePath))
                        {
                            importFiles.Add(fileInfo);
                            _knownFiles[filePath] = fileInfo;
                        }
                        // Files that have changed
                        else if (_knownFiles.TryGetValue(filePath, out var known) &&
                                 known.LastChange != fileInfo.LastChange)
                        {
                            removeFiles.Add(known.Path);
                            importFiles.Add(fileInfo);
                            _knownFiles[filePath] = fileInfo;
                        }
                    }

                    // Missing files
                    var missingFiles = _knownFiles.Keys.Where(filePath => !matchedFiles.ContainsKey(filePath)).ToList();
                    foreach (var filePath in missingFiles)
                    {
                        removeFiles.Add(filePath);
                        _knownFiles.Remove(filePath);
                    }
                });

                var token = _tokenSource.Token;
                if (token.IsCancellationRequested) return;

                if (removeFiles.Any()) Subscriber.OnItemsRemoved(this, removeFiles);
                if (importFiles.Any()) await Subscriber.OnItemsAddedAsync(this, importFiles, token);
            }
            catch (Exception ex)
            {
                UnityLogger.Instance.Error(ex.Message);
            }

            if(State == Refreshing) State.Value = Ok;
            _rescanRunning = false;
        }
        
        // private async Task InitializeWatcherAsync()
        // {
        //     if (_watcher == null)
        //     {
        //         await Task.Run(() =>
        //         {
        //             _watcher = _fileSystem.CreateWatcher(SupportedFilePattern, _config.ScanSubDirectories);
        //             _watcher.FileAdded += WatcherOnFileAdded;
        //             _watcher.FileRemoved += WatcherOnFileRemoved;
        //         });
        //     }
        // }

        // private void WatcherOnFileRemoved(object sender, string e) => ResetTimer();
        // private void WatcherOnFileAdded(object sender, string e) => ResetTimer();

        // private async void TimerOnElapsed(object sender, ElapsedEventArgs e)
        // {
        //     while (_rescanRunning)
        //     {
        //         await Task.Delay(200);
        //     }
        //     
        //     await Task.Run(RescanItemsAsync);
        // }
        
        // private void ResetTimer()
        // {
        //     lock (_timer)
        //     {
        //         _timer.Stop();
        //         _timer.Start();
        //     }
        // }
        //
        // public override void Dispose()
        // {
        //     _watcher?.Dispose();
        //     _timer?.Dispose();
        // }

        public override void Dispose() => _tokenSource?.Cancel();

        public async Task OnDeletedAsync()
        {
            State.Value = Deleting;
            
            Dispose();
            
            while (_rescanRunning)
            {
                await Task.Delay(10);
            }
            
            Subscriber.OnItemsRemoved(this, _knownFiles.Keys.ToList());
        }
    }
}