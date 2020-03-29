using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using StlVault.Config;
using StlVault.Util;
using StlVault.Util.Logging;

namespace StlVault.Services
{
    internal class Library : ILibrary, ITagIndex
    {
        private static readonly ILogger Logger = UnityLogger.Instance;
        private static readonly char[] Separators = {'_', '-', ' ', '.', '(', ')', '+'};
        private static readonly char[] Digits = {'0', '1', '2', '3', '4', '5', '6', '7', '8', '9'};
        
        private readonly Dictionary<string, ItemPreviewMetadata> _metaData =
            new Dictionary<string, ItemPreviewMetadata>();

        private readonly ArrayTrie _trie = new ArrayTrie();
        private readonly IPreviewBuilder _builder;

        public Library(IPreviewBuilder builder)
        {
            _builder = builder;
        }

        private static bool IsOnBlackList(string tag) => tag == "repaired" || tag == "stl";

        public IReadOnlyList<ItemPreviewMetadata> GetItemPreviewMetadata(IReadOnlyList<string> filters)
        {
            lock (_metaData)
            {
                return _metaData.Values.Matching(filters);
            }
        }

        public async Task ImportRangeAsync(ImportFolderConfig folderConfig, IReadOnlyCollection<string> filesToImport)
        {
            lock (_metaData)
            {
                var importFiles = filesToImport
                    .Select(file => new ItemPreviewMetadata(file, GenerateTags(folderConfig, file), folderConfig))
                    .ToList();

                foreach (var fileData in importFiles)
                {
                    var filePath = fileData.StlFilePath;
                    if (_metaData.ContainsKey(filePath)
                        && _metaData[filePath].ImportFolderPath.Length <= fileData.ImportFolderPath.Length) continue;

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

            await Task.Delay(2000);
        }
        
        private static IReadOnlyList<string> GenerateTags(ImportFolderConfig folderConfig, string fullFilePath)
        {
            var rootPath = folderConfig.FullPath;
            var subDir = fullFilePath.Substring(rootPath.Length);
            var fileName = Path.GetFileNameWithoutExtension(fullFilePath);

            return BuildDumbTags(GetTagGenerationString(folderConfig.AutoTagMode))
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
        
        private static IEnumerable<string> BuildDumbTags(string file)
        {
            return file?.Split(Path.DirectorySeparatorChar)
                .SelectMany(name => name.Trim().Split(Separators, StringSplitOptions.RemoveEmptyEntries))
                .Select(tag => tag.Trim().ToLowerInvariant().Trim(Digits))
                .Where(tag => tag.Length > 2 && !IsOnBlackList(tag));
        }

        public Task RemoveRangeAsync(ImportFolderConfig folder, IReadOnlyCollection<string> filesToRemove)
        {
            return Task.CompletedTask;
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
    }
}