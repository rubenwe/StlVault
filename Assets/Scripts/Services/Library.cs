using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using StlVault.Config;
using StlVault.Messages;
using StlVault.Util;
using StlVault.Util.FileSystem;
using StlVault.Util.Logging;
using StlVault.Util.Messaging;
using StlVault.Util.Stl;
using StlVault.Util.Tags;
using StlVault.ViewModels;
using UnityEngine;
using ILogger = StlVault.Util.Logging.ILogger;

namespace StlVault.Services
{
    internal class Library : ILibrary, IFileSourceSubscriber
    {
        private static readonly ILogger Logger = UnityLogger.Instance;
        
        [NotNull] private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
        [NotNull] private readonly List<PreviewList> _previewStreams = new List<PreviewList>();
        [NotNull] private readonly TagManager _tagManager = new TagManager();
        [NotNull] private readonly ItemPreviewModelSet _previewModels;

        [NotNull] private readonly IMessageRelay _relay;
        [NotNull] private readonly IConfigStore _configStore;
        [NotNull] private readonly IPreviewBuilder _previewBuilder;
        [NotNull] private readonly IPreviewImageStore _previewStore;

        public ushort Parallelism { get; set; } = 1;

        public Library(
            [NotNull] IConfigStore configStore,
            [NotNull] IPreviewBuilder builder,
            [NotNull] IPreviewImageStore previewStore, 
            [NotNull] IMessageRelay relay)
        {
            _previewBuilder = builder ?? throw new ArgumentNullException(nameof(builder));
            _configStore = configStore ?? throw new ArgumentNullException(nameof(configStore));
            _previewStore = previewStore ?? throw new ArgumentNullException(nameof(previewStore));
            _relay = relay ?? throw new ArgumentNullException(nameof(relay));

            _previewModels = new ItemPreviewModelSet(_previewStore, relay);
        }

        public IPreviewList GetItemPreviewMetadata(IReadOnlyList<string> filters)
        {
            var lowerFilters = filters.Select(f => f.ToLowerInvariant()).ToList();

            var previewList = new PreviewList(
                list => _lock.Write(() => _previewStreams.Remove(list)),
                items => _tagManager.Filter(items, filters));

            _lock.Read(() => previewList.AddFiltered(_previewModels));
            _lock.Write(() => _previewStreams.Add(previewList));

            return previewList;
        }

        public void AddTag(IEnumerable<string> hashes, string tag) => UpdateTags(TagAction.Add, hashes, tag);
        public void RemoveTag(IEnumerable<string> hashes, string tag) => UpdateTags(TagAction.Remove, hashes, tag);

        public async Task RotateAsync(ItemPreviewModel previewModel, Vector3 newRotation)
        {
            try
            {
                IFileSource source = null;
                string filePath = null;
                _lock.Read(() => (source, filePath) = _previewModels.TryGetFileSource(previewModel));
            
                if (source == null || filePath == null) return;
                var (_, geoInfo, resolution) = await RebuildPreview(
                    source, filePath,
                    newRotation, previewModel.GeometryInfo.Value.Scale,
                    previewModel.FileHash);

                _lock.Write(() =>
                {
                    previewModel.PreviewResolution = resolution;
                    previewModel.GeometryInfo.Value = geoInfo;
                });

                previewModel.OnPreviewChanged();
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error while rotating `{0}`", previewModel.Name);
            }
        }

        public IEnumerable<ItemPreviewModel> GetAllItems() => _previewModels;
        
        public async Task<Mesh> GetMeshAsync(ItemPreviewModel previewModel)
        {
            IFileSource source = null;
            string filePath = null;
            _lock.Read(() => (source, filePath) = _previewModels.TryGetFileSource(previewModel));
            
            if (source == null || filePath == null) return null;
            
            var fileName = Path.GetFileName(filePath);

            var fileBytes = await source
                .GetFileBytesAsync(filePath)
                .Timed("Reading file {0}", fileName);

            var (mesh, _) = await StlImporter
                .ImportMeshAsync(fileName, fileBytes, centerVertices: false, computeHash: false)
                .Timed("Imported {0}", fileName);

            return mesh;
        }

        public bool TryGetLocalPath(ItemPreviewModel previewModel, out string localPath)
        {
            try
            {
                IFileSource source = null;
                string filePath = null;
                _lock.Read(() => (source, filePath) = _previewModels.TryGetFileSource(previewModel));

                localPath = Path.Combine(source.Id, filePath);
                return File.Exists(localPath);
            }
            catch
            {
                localPath = null;
            }

            return false;
        }

        public Vector3 GetImportRotation(ItemPreviewModel previewModel)
        {
            var rotation = Vector3.zero;
            _lock.Read(() =>
            {
                var (source, _) = _previewModels.TryGetFileSource(previewModel);
                rotation = source.Config.Rotation ?? Vector3.zero;
            });

            return rotation;
        }

        private enum TagAction
        {
            Add,
            Remove
        }

        private void UpdateTags(TagAction action, IEnumerable<string> hashes, string tag)
        {
            _lock.Write(() =>
            {
                foreach (var hash in hashes)
                {
                    if (_previewModels.TryGetValue(hash, out var item))
                    {
                        if (action == TagAction.Add)
                        {
                            item.Tags.Add(tag);
                            _tagManager.Add(tag);
                        }
                        else item.Tags.Remove(tag);
                    }
                }
            });
        }

        /// <summary>
        /// Called during initial creation of scene graph.
        /// Must finish before others can access it.
        /// </summary>
        [SuppressMessage("ReSharper", "InconsistentlySynchronizedField")]
        public async Task InitializeAsync()
        {
            var metaData = await _configStore.LoadAsyncOrDefault<MetaData>()
                .Timed("Loading metadata file");
            
            _tagManager.Add(metaData.SelectMany(info => info.Tags).ToList());
            _previewModels.Initialize(metaData);
        }

        public IReadOnlyList<TagSearchResult> GetRecommendations(IEnumerable<string> currentFilters, string search)
        {
            var filters = currentFilters.Select(filter => filter.ToLowerInvariant()).ToList();
            var newFilter = search.ToLowerInvariant();
            
            return _lock.Read(() => _tagManager.GetRecommendations(_previewModels, filters, newFilter));
        }

        public async Task OnItemsAddedAsync(
            IFileSource source, 
            IReadOnlyCollection<IFileInfo> addedFiles, 
            CancellationToken token)
        {
            if (addedFiles?.Any() != true) return;

            List<IFileInfo> itemsForImport = null;

            _lock.Read(() =>
            {
                var currentSourceFiles = _previewModels.GetKnownFiles(source);
                itemsForImport = addedFiles.Where(file =>
                        !currentSourceFiles.TryGetValue(file.Path, out var info) ||
                        info.LastChange != file.LastChange)
                    .ToList();
            });

            await ImportBatched(source, itemsForImport, token);
        }

        private async Task ImportBatched(
            IFileSource source, 
            List<IFileInfo> itemsForImport, 
            CancellationToken token)
        {
            var current = -1;
            var tasks = new Task[Parallelism];
            for (var i = 0; i < tasks.Length; i++)
            {
                tasks[i] = Task.Run(async () =>
                {
                    int next;
                    while ((next = Interlocked.Increment(ref current)) < itemsForImport.Count)
                    {
                        if (token.IsCancellationRequested) return;
                        
                        await ImportFile(source, itemsForImport[next], token);
                    }
                }, token);
            }

            await Task.WhenAll(tasks);
            
            _relay.Send(this, new ProgressMessage{Text = $"Finished importing from `{source.DisplayName}`"});
        }

        private async Task ImportFile(
            IFileSource source,
            IFileInfo file, 
            CancellationToken token)
        {
            var fileName = Path.GetFileName(file.Path);
            
            try
            {
                var scale = source.Config.Scale;
                var rotation = source.Config.Rotation;

                var (hash, geoInfo, resolution) = await RebuildPreview(source, file.Path, rotation, scale);

                var tags = source.GetTags(file.Path);
                var previewInfo = new PreviewInfo
                {
                    ItemName = fileName,
                    FileHash = hash,
                    Tags = tags.ToHashSet(),
                    Resolution = resolution
                };

                _lock.Write(() =>
                { 
                    if (token.IsCancellationRequested) return;
                    
                    _tagManager.Add(tags);
                    var model = _previewModels.AddOrUpdate(source, file, previewInfo, geoInfo);
                    _previewStreams.ForEach(stream => stream.AddFiltered(model));
                    _relay.Send(this, new ProgressMessage {Text = $"Imported `{fileName}`"});
                });
            }
            catch (Exception ex)
            {
                var text = $"Error while importing `{fileName}`";
                _relay.Send<ProgressMessage>(new ProgressMessage {Text = text});
                Logger.Error(ex, text);
            }
        }

        private async Task<(string hash, GeometryInfo geoInfo, int resolution)> RebuildPreview(
            IFileSource source, string filePath,
            ConfigVector3? rotation, ConfigVector3? scale, 
            string knownHash = null)
        {
            var fileName = Path.GetFileName(filePath);

            var fileBytes = await source
                .GetFileBytesAsync(filePath)
                .Timed("Reading file {0}", fileName);

            var (mesh, hash) = await StlImporter
                .ImportMeshAsync(fileName, fileBytes, computeHash: knownHash == null)
                .Timed("Imported {0}", fileName);

            var geoInfo = await GeometryInfo.FromMeshAsync(mesh, rotation, scale);

            var (previewData, resolution) = await _previewBuilder
                .GetPreviewImageDataAsync(mesh, rotation)
                .Timed("Building preview for {0}", fileName);

            StlImporter.Destroy(mesh);

            await _previewStore.StorePreviewAsync(hash ?? knownHash, previewData);

            return (hash, geoInfo, resolution);
        }


        public void OnItemsRemoved(IFileSource source, IReadOnlyCollection<string> removedItems)
        {
            _lock.Write(() =>
            {
                var itemsToRemove = new HashSet<ItemPreviewModel>();
                using (NotifyMassSelection())
                {
                    foreach (var removedItem in removedItems)
                    {
                        var model = _previewModels.RemoveOrUpdate(source, removedItem);
                        if (model != null) itemsToRemove.Add(model);
                    }
                }

                _previewStreams.ForEach(stream => stream.RemoveRange(itemsToRemove));
            });
        }
        
        private IDisposable NotifyMassSelection()
        {
            _relay.Send<MassSelectionStartingMessage>(this);
            return Disposable.FromAction(() => _relay.Send<MassSelectionFinishedMessage>(this));
        }

        public async Task StoreChangesAsync()
        {
            await Task.Run(() =>
            {
                try
                {
                    _lock.EnterReadLock();
                    var metaData = _previewModels.GetMetaData();
                    
                    _configStore.StoreAsync(metaData, true)
                        .Timed("Writing metadata file")
                        .Wait();
                }
                finally
                {
                    _lock.ExitReadLock();
                }
            });
        }
    }
}
    