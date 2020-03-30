using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using JetBrains.Annotations;
using StlVault.Util;

namespace StlVault.Services
{
    internal class Library : ILibrary, ITagIndex, IFileSourceSubscriber
    {
        private readonly List<PreviewList> _previewStreams = new List<PreviewList>();
        private readonly Dictionary<string, ItemPreviewMetadata> _metaData =
            new Dictionary<string, ItemPreviewMetadata>();

        [NotNull] private readonly ArrayTrie _trie = new ArrayTrie();
        [NotNull] private readonly MD5 _md5 = MD5.Create();
        [NotNull] private readonly IPreviewBuilder _builder;
        
        public Library([NotNull] IPreviewBuilder builder)
        {
            _builder = builder ?? throw new ArgumentNullException(nameof(builder));
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

        public async Task OnInitializeAsync(IFileSource source, IReadOnlyCollection<string> files)
        {
            lock (_metaData)
            {
                Import(source, files);
            }
            
            await Task.Delay(800);
        }

        public async Task OnItemsAddedAsync(IFileSource source, IReadOnlyCollection<string> addedFiles)
        {
            lock (_metaData)
            {
                Import(source, addedFiles);
            }

            await Task.Delay(800);
        }
        
        public async Task OnItemsRemovedAsync(IFileSource source, IReadOnlyCollection<string> removedItems)
        {
            lock (_metaData)
            {
                UnImport(removedItems);
            }

            await Task.Delay(50);
        }

        private void UnImport(IReadOnlyCollection<string> removedItems)
        {
            var itemsToRemove = new HashSet<ItemPreviewMetadata>();
            foreach (var removedItem in removedItems)
            {
                if (_metaData.TryGetValue(removedItem, out var data))
                {
                    itemsToRemove.Add(data);
                    _metaData.Remove(removedItem);
                }
            }
            
            _previewStreams.ForEach(stream => stream.RemoveRange(itemsToRemove));
        }

        private void Import(IFileSource source, IReadOnlyCollection<string> addedFiles)
        {
            var importFiles = addedFiles
                .Select(file => new ItemPreviewMetadata(file, source.GetTags(file)))
                .ToList();

            _previewStreams.ForEach(stream => stream.AddFiltered(importFiles));
            
            foreach (var fileData in importFiles)
            {
                var filePath = fileData.StlFilePath;

                _metaData[filePath] = fileData;

                lock (_trie)
                {
                    foreach (var tag in fileData.Tags)
                    {
                        _trie.Insert(tag);
                    }
                }
            }
        }

    }
}