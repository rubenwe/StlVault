using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using StlVault.Config;
using StlVault.Util;
using StlVault.Util.FileSystem;
using StlVault.Util.Messaging;
using StlVault.Util.Stl;
using StlVault.ViewModels;
using UnityEngine;

namespace StlVault.Services
{
    internal class Library : ILibrary, IFileSourceSubscriber
    {
        private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
        private readonly List<PreviewList> _previewStreams = new List<PreviewList>();
        private readonly ItemPreviewModelSet _previewModels;

        [NotNull] private readonly IConfigStore _configStore;
        [NotNull] private readonly IPreviewBuilder _previewBuilder;
        [NotNull] private readonly IPreviewImageStore _previewStore;
        [NotNull] private readonly ArrayTrie _trie = new ArrayTrie();

        public ushort Parallelism { get; set; } = 1;

        public Library(
            [NotNull] IConfigStore configStore,
            [NotNull] IPreviewBuilder builder,
            [NotNull] IPreviewImageStore previewStore, 
            [NotNull] IMessageRelay relay)
        {
            if (relay == null) throw new ArgumentNullException(nameof(relay));
            
            _previewBuilder = builder ?? throw new ArgumentNullException(nameof(builder));
            _configStore = configStore ?? throw new ArgumentNullException(nameof(configStore));
            _previewStore = previewStore ?? throw new ArgumentNullException(nameof(previewStore));
            
            _previewModels = new ItemPreviewModelSet(_previewStore, relay);
        }

        public IPreviewList GetItemPreviewMetadata(IReadOnlyList<string> filters)
        {
            var lowerFilters = filters.Select(f => f.ToLowerInvariant()).ToList();
            bool MatchesFilters(ItemPreviewModel m) => lowerFilters.All(m.Tags.Contains);

            var previewList = new PreviewList(
                list => WriteLocked(() => _previewStreams.Remove(list)),
                items => items.Where(MatchesFilters).ToList());

            ReadLocked(() => previewList.AddFiltered(_previewModels));
            WriteLocked(() => _previewStreams.Add(previewList));

            return previewList;
        }

        public void AddTag(IEnumerable<string> hashes, string tag)
        {
            UpdateTags(TagAction.Add, hashes, tag);
        }

        public void RemoveTag(IEnumerable<string> hashes, string tag)
        {
            UpdateTags(TagAction.Remove, hashes, tag);
        }

        public async Task RotateAsync(ItemPreviewModel previewModel, Vector3 newRotation)
        {
            IFileSource source = null;
            string filePath = null;
            ReadLocked(() => (source, filePath) = _previewModels.TryGetFileSource(previewModel));
            
            if (source == null || filePath == null) return;
            
            var (_, geoInfo, resolution) = await RebuildPreview(
                source, filePath, 
                newRotation, previewModel.GeometryInfo.Value.Scale, 
                previewModel.FileHash);
            
            WriteLocked(() =>
            {
                previewModel.PreviewResolution = resolution;
                previewModel.GeometryInfo.Value = geoInfo;
            });
            
            previewModel.OnPreviewChanged();
        }

        private enum TagAction
        {
            Add,
            Remove
        }

        private void UpdateTags(TagAction action, IEnumerable<string> hashes, string tag)
        {
            WriteLocked(() =>
            {
                foreach (var hash in hashes)
                {
                    if (_previewModels.TryGetValue(hash, out var item))
                    {
                        if (action == TagAction.Add)
                        {
                            item.Tags.Add(tag);
                            _trie.Insert(tag);
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
            
            _trie.Insert(metaData.SelectMany(info => info.Tags).ToList());
            _previewModels.Initialize(metaData);
        }

        public IReadOnlyList<TagSearchResult> GetRecommendations(IEnumerable<string> currentFilters, string search)
        {
            List<TagSearchResult> recommendations = null;

            ReadLocked(() =>
            {
                var known = new HashSet<string>(currentFilters);

                recommendations = _trie.Find(search.ToLowerInvariant())
                    .Select(rec => new TagSearchResult(rec.word, _previewModels.Matching(known.Append(rec.word).ToList()).Count))
                    .Where(result => result.MatchingItemCount > 0)
                    .Where(result => !known.Contains(result.SearchTag))
                    .OrderByDescending(result => result.MatchingItemCount)
                    .ToList();
            });

            return recommendations;
        }

        public async Task OnItemsAddedAsync(IFileSource source, IReadOnlyCollection<IFileInfo> addedFiles)
        {
            if (addedFiles?.Any() != true) return;

            List<IFileInfo> itemsForImport = null;

            ReadLocked(() =>
            {
                var currentSourceFiles = _previewModels.GetKnownFiles(source);
                itemsForImport = addedFiles.Where(file =>
                        !currentSourceFiles.TryGetValue(file.Path, out var info) ||
                        info.LastChange != file.LastChange)
                    .ToList();
            });

            await ImportBatched(source, itemsForImport);
        }

        private async Task ImportBatched(IFileSource source, List<IFileInfo> itemsForImport)
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
                        await ImportFile(source, itemsForImport[next]);
                    }
                });
            }

            await Task.WhenAll(tasks);
        }

        private async Task ImportFile(
            IFileSource source,
            IFileInfo file)
        {
            var scale = source.Config.Scale;
            var rotation = source.Config.Rotation;

            var (hash, geoInfo, resolution) = await RebuildPreview(source, file.Path, rotation, scale);

            var tags = source.GetTags(file.Path);
            var previewInfo = new PreviewInfo
            {
                ItemName = Path.GetFileName(file.Path),
                FileHash = hash,
                Tags = tags.ToHashSet(),
                Resolution = resolution
            };

            WriteLocked(() =>
            {
                _trie.Insert(tags);
                var model = _previewModels.AddOrUpdate(source, file, previewInfo, geoInfo);
                _previewStreams.ForEach(stream => stream.AddFiltered(model));
            });
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
            WriteLocked(() =>
            {
                var itemsToRemove = new HashSet<ItemPreviewModel>();
                foreach (var removedItem in removedItems)
                {
                    var model = _previewModels.RemoveOrUpdate(source, removedItem);
                    if(model != null) itemsToRemove.Add(model);
                }

                _previewStreams.ForEach(stream => stream.RemoveRange(itemsToRemove));
            });
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

        private void WriteLocked(Action writingAction)
        {
            try
            {
                _lock.EnterWriteLock();
                writingAction.Invoke();
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        private void ReadLocked(Action readingAction)
        {
            try
            {
                _lock.EnterReadLock();
                readingAction.Invoke();
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }
    }
}
    