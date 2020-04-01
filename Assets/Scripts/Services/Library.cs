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
using UnityEngine;

namespace StlVault.Services
{
    internal class Library : ILibrary, ITagIndex, IFileSourceSubscriber
    {
        private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
        private readonly List<PreviewList> _previewStreams = new List<PreviewList>();
        private MetaData _metaData;
        private KnownFiles _knownFiles;
        
        [NotNull] private readonly IConfigStore _configStore;
        [NotNull] private readonly IPreviewBuilder _previewBuilder;
        [NotNull] private readonly IPreviewImageStore _previewStore;
        [NotNull] private readonly ArrayTrie _trie = new ArrayTrie();

        public ushort Parallelism { get; set; } = 1;
        
        public Library(
            [NotNull] IConfigStore configStore,
            [NotNull] IPreviewBuilder builder,
            [NotNull] IPreviewImageStore previewStore)
        {
            _previewBuilder = builder ?? throw new ArgumentNullException(nameof(builder));
            _configStore = configStore ?? throw new ArgumentNullException(nameof(configStore));
            _previewStore = previewStore ?? throw new ArgumentNullException(nameof(previewStore));
        }
        
        public IPreviewList GetItemPreviewMetadata(IReadOnlyList<string> filters)
        {
            var previewList = new PreviewList(
                list => WriteLocked(() => _previewStreams.Remove(list)), 
                items =>  items.Matching(filters));
            
            ReadLocked(() => previewList.AddFiltered(_metaData.Values));
            WriteLocked(() => _previewStreams.Add(previewList));
            
            return previewList;
        }

        /// <summary>
        /// Called during initial creation of scene graph.
        /// Must finish before others can access it.
        /// </summary>
        [SuppressMessage("ReSharper", "InconsistentlySynchronizedField")]
        public async Task InitializeAsync()
        {
            _metaData = await _configStore.LoadAsyncOrDefault<MetaData>();
            _knownFiles =  await _configStore.LoadAsyncOrDefault<KnownFiles>();

            foreach (var (hash, previewInfo) in _metaData)
            {
                previewInfo.FileHash = hash;
                foreach (var tag in previewInfo.Tags)
                {
                    _trie.Insert(tag);
                }
            }

            foreach (var items in _knownFiles.Values)
            foreach (var (itemPath, info) in items)
            {
                info.Path = itemPath;
            }
        }
      
        public IReadOnlyList<TagSearchResult> GetRecommendations(IEnumerable<string> currentFilters, string search)
        {
            List<TagSearchResult> recommendations = null;
            
            ReadLocked(() =>
            {
                var known = new HashSet<string>(currentFilters);
                
                recommendations = _trie.Find(search.ToLowerInvariant())
                    .Select(rec => new TagSearchResult(rec.word, _metaData.Values.Matching(known.Append(rec.word)).Count))
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
            Dictionary<string, ImportedFileInfo> currentSourceFiles = null;
            
            ReadLocked(() =>
            {
                currentSourceFiles = _knownFiles[source.DisplayName];
                itemsForImport = addedFiles.Where(file => 
                        !currentSourceFiles.TryGetValue(file.Path, out var info) || 
                        info.LastChange != file.LastChange)
                    .ToList();
            });

            await ImportBatched(source, itemsForImport, currentSourceFiles);
            await StoreChangesAsync();
        }

        private async Task ImportBatched(IFileSource source, List<IFileInfo> itemsForImport, Dictionary<string, ImportedFileInfo> currentSourceFiles)
        {
            var current = 0;
            var tasks = new Task[Parallelism];
            for (var i = 0; i < tasks.Length; i++)
            {
                tasks[i] = Task.Run(async () =>
                {
                    int next;
                    while ((next = Interlocked.Increment(ref current)) < itemsForImport.Count)
                    {
                        await ImportFile(source, itemsForImport[next], currentSourceFiles);
                    }
                });
            }
            
            await Task.WhenAll(tasks);
        }

        private async Task ImportFile(
            IFileSource source,
            IFileInfo file,
            Dictionary<string, ImportedFileInfo> currentSourceFiles)
        {
            var fileName = Path.GetFileName(file.Path);
            if (fileName == null) return;

            var fileBytes = await source
                .GetFileBytesAsync(file.Path)
                .Timed("Reading file {0}", fileName);

            var (mesh, hash) = await StlImporter
                .ImportMeshAsync(fileName, fileBytes)
                .Timed("Imported {0}", fileName);

            var previewData = await _previewBuilder
                .GetPreviewImageDataAsync(mesh, source.Config.Rotation)
                .Timed("Building preview for {0}", fileName);

            StlImporter.Destroy(mesh);

            await _previewStore.StorePreviewAsync(hash, previewData);

            var tags = source.GetTags(file.Path);
            var previewInfo = new PreviewInfo(fileName, hash, tags.ToHashSet());

            WriteLocked(() =>
            {
                _trie.Insert(tags);
                _metaData[hash] = previewInfo;
                currentSourceFiles[file.Path] = new ImportedFileInfo(file, hash);
                _previewStreams.ForEach(stream => stream.AddFiltered(previewInfo));
            });
        }

        public async Task OnItemsRemovedAsync(IFileSource source, IReadOnlyCollection<string> removedItems)
        {
            WriteLocked(() =>
            {
                var itemsToRemove = new HashSet<PreviewInfo>();
                var currentSourceFiles = _knownFiles[source.DisplayName];
                foreach (var removedItem in removedItems)
                {
                    if (currentSourceFiles.TryGetValue(removedItem, out var info))
                    {
                        if (_metaData.TryGetValue(info.Hash, out var data))
                        {
                            itemsToRemove.Add(data);
                            _metaData.Remove(data.FileHash);
                            currentSourceFiles.Remove(removedItem);
                        }
                    }
                }
            
                _previewStreams.ForEach(stream => stream.RemoveRange(itemsToRemove));
            });
            
            await StoreChangesAsync();
        }

        private async Task StoreChangesAsync()
        {
            await Task.Run(() =>
            {
                try
                {
                    _lock.EnterReadLock();
                    var t1 = _configStore.StoreAsync(_knownFiles);
                    var t2 = _configStore.StoreAsync(_metaData);
                    Task.WhenAll(t1, t2).Wait();
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