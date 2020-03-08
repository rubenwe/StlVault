using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using StlVault.Config;
using StlVault.Util;
using StlVault.Views;
using UnityEngine.UIElements;

namespace StlVault.AppModel
{
    internal class Library : ILibrary, ITagIndex
    {
        private static readonly char[] Separators = new[]{'_', '-', ' ', '(', ')', '+'};
        private readonly Dictionary<string, ItemPreviewMetadata> _metaData = new Dictionary<string, ItemPreviewMetadata>();
        
        private readonly IConfigStore _store;
        private readonly ArrayTrie _trie = new ArrayTrie();

        public Library(IConfigStore store)
        {
            _store = store;
        }

        public async Task InitializeAsync()
        {
            var folders = await _store.TryLoadAsync<ImportFoldersConfig>();
            
            foreach (var folderConfig in folders)
            {
                var path = folderConfig.FullPath;
                if (!Directory.Exists(path)) continue;

                var topDirectoryOnly = folderConfig.ScanSubDirectories 
                    ? SearchOption.AllDirectories 
                    : SearchOption.TopDirectoryOnly;
                
                var importFiles = Directory.GetFiles(path, "*.stl", topDirectoryOnly)
                    .Select(file => new ItemPreviewMetadata(file, BuildDumbTags(file.Substring(path.Length)), folderConfig))
                    .ToList();

                foreach (var fileData in importFiles)
                {
                    var filePath = fileData.StlFilePath;
                    if (_metaData.ContainsKey(filePath) 
                        && _metaData[filePath].ImportFolderPath.Length <= fileData.ImportFolderPath.Length) continue;
                    
                    _metaData[filePath] = fileData;
                }
            }

            InitializeIndex(_metaData.Values);
            
            IReadOnlyList<string> BuildDumbTags(string file)
            {
                return file?.Split(Path.DirectorySeparatorChar)
                    .SelectMany(name => name.Trim().Split(Separators, StringSplitOptions.RemoveEmptyEntries))
                    .Select(tag => tag.Trim().ToLowerInvariant())
                    .Where(tag => tag.Length > 2 && !IsOnBlackList(tag))
                    .ToList();
            }
        }

        private static bool IsOnBlackList(string tag) => tag == "repaired" || tag == ".stl";

        public IReadOnlyList<ItemPreviewMetadata> GetItemPreviewMetadata(IReadOnlyList<string> filters)
        {
            return _metaData.Values.Matching(filters);
        }

        public IOrderedEnumerable<TagSearchResult> GetRecommendations(IEnumerable<string> currentFilters, string search)
        {
            var known = new HashSet<string>(currentFilters);
            
            return _trie.Find(search.ToLowerInvariant())
                .Select(rec => new TagSearchResult(rec, _metaData.Values.Matching(known.Append(rec)).Count))
                .Where(result => result.MatchingItemCount > 0)
                .Where(result => !known.Contains(result.SearchTag))
                .OrderByDescending(result => result.MatchingItemCount);
        }

        private void InitializeIndex(IEnumerable<ItemPreviewMetadata> metaData)
        {
            foreach (var tag in metaData.SelectMany(d => d.Tags))
            {
                _trie.Insert(tag);
            }
        }
    }
}