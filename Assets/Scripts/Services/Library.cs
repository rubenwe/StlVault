using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using StlVault.Util;
using StlVault.Util.FileSystem;
using StlVault.Util.Stl;

namespace StlVault.Services
{
    internal class Library : ILibrary, ITagIndex, IFileSourceSubscriber
    {
        private readonly List<PreviewList> _previewStreams = new List<PreviewList>();
        private readonly Dictionary<string, PreviewInfo> _metaData = new Dictionary<string, PreviewInfo>();

        [NotNull] private readonly ArrayTrie _trie = new ArrayTrie();
        [NotNull] private readonly IPreviewBuilder _previewBuilder;
        [NotNull] private readonly IConfigStore _configStore;
        [NotNull] private readonly IPreviewImageStore _previewStore;

        private KnownFiles _knownFiles;
        
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
            lock (_metaData)
            {
                var previewList = new PreviewList(
                    list => _previewStreams.Remove(list), 
                    items =>  items.Matching(filters));

                _previewStreams.Add(previewList);

                previewList.AddFiltered(_metaData.Values);

                return previewList;
            }
        }

        public async Task InitializeAsync()
        {
            _knownFiles = await _configStore.LoadAsyncOrDefault<KnownFiles>();
        }
      
        public IOrderedEnumerable<TagSearchResult> GetRecommendations(IEnumerable<string> currentFilters, string search)
        {
            var known = new HashSet<string>(currentFilters);

            lock (_trie)
            {
                return _trie.Find(search.ToLowerInvariant())
                    .Select(rec => new TagSearchResult(rec.word, rec.occurrences))
                    .Where(result => result.MatchingItemCount > 0)
                    .Where(result => !known.Contains(result.SearchTag))
                    .OrderByDescending(result => result.MatchingItemCount);
            }
        }

        public async Task OnInitializeAsync(IFileSource source, IReadOnlyCollection<IFileInfo> files)
        {
            await ImportAsync(source, files);
        }

        public async Task OnItemsAddedAsync(IFileSource source, IReadOnlyCollection<IFileInfo> addedFiles)
        { 
            await ImportAsync(source, addedFiles);
        }
        
        public async Task OnItemsRemovedAsync(IFileSource source, IReadOnlyCollection<string> removedItems)
        {
            
            UnImport(removedItems);

            await Task.Delay(50);
        }

        private void UnImport(IReadOnlyCollection<string> removedItems)
        {
            var itemsToRemove = new HashSet<PreviewInfo>();
            lock (_metaData)
            {
                foreach (var removedItem in removedItems)
                {
                    if (_metaData.TryGetValue(removedItem, out var data))
                    {
                        itemsToRemove.Add(data);
                        _metaData.Remove(removedItem);
                    }
                }
            }
            
            _previewStreams.ForEach(stream => stream.RemoveRange(itemsToRemove));
        }

        private async Task ImportAsync(IFileSource source, IReadOnlyCollection<IFileInfo> addedFiles)
        {
            var knownFiles = _knownFiles[source.DisplayName]
                .ToDictionary(info => info.Path);

            var itemsForImport = addedFiles
                .Where(file => !knownFiles.TryGetValue(file.Path, out var info) || new BasicFileInfo(file) != info)
                .Select(file => file.Path)
                .ToList();

            foreach (var filePath in itemsForImport)
            {
                var fileName = Path.GetFileName(filePath);
                if (fileName == null) continue;
                
                var fileBytes = await source
                    .GetFileBytesAsync(filePath)
                    .Timed("Reading file {0}", fileName);
                
                var (mesh, hash) = await StlImporter
                    .ImportMeshAsync(fileName, fileBytes)
                    .Timed("Imported {0}", fileName);

                var previewData = await _previewBuilder
                    .GetPreviewImageDataAsync(mesh, source.Config.Rotation)
                    .Timed("Building preview for {0}", fileName);

                await _previewStore.StorePreviewAsync(hash, previewData);

                var tags = source.GetTags(filePath);
                var previewInfo = new PreviewInfo(fileName, hash, tags);
                
                lock (_trie)
                {
                    foreach (var tag in tags)
                    {
                        _trie.Insert(tag);
                    }
                }

                lock (_metaData)
                {
                    _metaData[$"{source.DisplayName}:{filePath}"] = previewInfo;
                }
                
                _previewStreams.ForEach(stream => stream.AddFiltered(previewInfo));
            }
        }
    }

    internal class KnownFiles : Dictionary<string, HashSet<BasicFileInfo>>
    {
        public new HashSet<BasicFileInfo> this[string sourceId]
        {
            get
            {
                if (!TryGetValue(sourceId, out var list))
                {
                    list = this[sourceId] = new HashSet<BasicFileInfo>();
                }

                return list;
            }
            private set => base[sourceId] = value;
        }
    }
}