using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using StlVault.Config;
using StlVault.Util;
using UnityEngine.UIElements;
using ILogger = StlVault.Util.ILogger;

namespace StlVault.AppModel
{
    internal class Library : ILibrary, ITagIndex
    {
        private static readonly ILogger Logger = UnityLogger.Instance;
        private static readonly char[] Separators = {'_', '-', ' ', '.', '(', ')', '+'};
        private static readonly char[] Digits = {'0', '1', '2', '3', '4', '5', '6', '7', '8', '9'};
        private readonly Dictionary<string, ItemPreviewMetadata> _metaData = new Dictionary<string, ItemPreviewMetadata>();
        
        private readonly IConfigStore _store;
        private readonly ArrayTrie _trie = new ArrayTrie();

        public Library(IConfigStore store, IPreviewBuilder builder)
        {
            _store = store;
        }

        public async Task InitializeAsync()
        {
            var config = await _store.LoadAsyncOrDefault<KnownItemsConfigFile>();
            
            var knownItems = config
                .ToLookup(info => info.FileLocation)
                .ToDictionary(grp => grp.Key, grp => grp.First());
            
            var folders = await _store.LoadAsyncOrDefault<ImportFoldersConfigFile>();
            
            foreach (var folderConfig in folders)
            {
                var path = folderConfig.FullPath;
                if (!Directory.Exists(path)) continue;

                var topDirectoryOnly = folderConfig.ScanSubDirectories 
                    ? SearchOption.AllDirectories 
                    : SearchOption.TopDirectoryOnly;

                var importFiles = new List<ItemPreviewMetadata>();
                foreach (var file in Directory.GetFiles(path, "*.stl", topDirectoryOnly))
                {
                    var tags = GenerateTags(folderConfig, file);
                    var metaData = new ItemPreviewMetadata(file, tags, folderConfig);
                }

                foreach (var fileData in importFiles)
                {
                    var filePath = fileData.StlFilePath;
                    if (_metaData.ContainsKey(filePath) 
                        && _metaData[filePath].ImportFolderPath.Length <= fileData.ImportFolderPath.Length) continue;
                    
                    _metaData[filePath] = fileData;
                }
            }

            InitializeIndex(_metaData.Values);

            IReadOnlyList<string> GenerateTags(ImportFolderConfig folder, string fullFilePath)
            {
                var rootPath = folder.FullPath;
                var subDir = fullFilePath.Substring(rootPath.Length);
                var fileName = Path.GetFileNameWithoutExtension(fullFilePath);
               
                return BuildDumbTags(GetTagGenerationString(folder.AutoTagMode))
                    .Append("folder: " + rootPath.ToLowerInvariant())
                    .ToList();

                string GetTagGenerationString(AutoTagMode mode)
                {
                    switch (mode)
                    {
                        case AutoTagMode.ExplodeAbsolutePath: return fullFilePath;
                        case AutoTagMode.ExplodeSubDirPath: return subDir;
                        case AutoTagMode.ExplodeFileName: return fileName;
                        default: return string.Empty;
                    }
                }
            }
            
            IEnumerable<string> BuildDumbTags(string file)
            {
                return file?.Split(Path.DirectorySeparatorChar)
                    .SelectMany(name => name.Trim().Split(Separators, StringSplitOptions.RemoveEmptyEntries))
                    .Select(tag => tag.Trim().ToLowerInvariant().Trim(Digits))
                    .Where(tag => tag.Length > 2 && !IsOnBlackList(tag));
            }
        }
        
        private static bool IsOnBlackList(string tag) => tag == "repaired" || tag == "stl";

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

    
    internal interface IPreviewBuilder
    {
    }
}