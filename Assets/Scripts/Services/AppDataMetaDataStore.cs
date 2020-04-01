using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using StlVault.Config;
using StlVault.Util;
using StlVault.Util.Logging;

namespace StlVault.Services
{
    internal class AppDataMetaDataStore : IMetaDataStore
    {
        private Dictionary<string, string> _altToMainHash;
        private Dictionary<string, HashSet<string>> _mainToAltHash;

        private static string MetaDataPath
        {
            get
            {
                var appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                return Path.Combine(appData, "StlVault", "MetaData");
            }
        }

        private static string HashMapFile => Path.Combine(MetaDataPath, "HashMap.json");
        private static string GetTagFile(string hash) => Path.Combine(MetaDataPath, "Tags", hash + ".json");

        private IReadOnlyList<string> GetTagsForItem(string hash) => ReadFile<List<string>>(GetTagFile(hash));

        private Dictionary<string, HashSet<string>> GetHashMap() =>
            ReadFile<Dictionary<string, HashSet<string>>>(HashMapFile);

        private static T ReadFile<T>(string filePath) where T : class
        {
            try
            {
                var text = File.ReadAllText(filePath);
                return JsonConvert.DeserializeObject<T>(text);
            }
            catch
            {
                return null;
            }
        }

        private static void SaveFile(string filePath, object itemToSave)
        {
            try
            {
                var dir = Path.GetDirectoryName(filePath) ?? Throw();
                Directory.CreateDirectory(dir);

                var json = JsonConvert.SerializeObject(itemToSave, Formatting.Indented);
                File.WriteAllText(filePath, json);
            }
            catch (Exception ex)
            {
                UnityLogger.Instance.Error("Could not write settings to {0}: {1}", filePath, ex.Message);
            }

            string Throw() => throw new InvalidDataException($"Could not get directory from file name {filePath}");
        }

        private void SaveHashMap() => SaveFile(HashMapFile, _mainToAltHash);
        private void SaveTagsForItem(string hash, List<string> tags) => SaveFile(GetTagFile(hash), tags);

        public async Task InitializeAsync()
        {
            _mainToAltHash = await Task.Run(GetHashMap) ?? new Dictionary<string, HashSet<string>>();
            _altToMainHash = new Dictionary<string, string>();
            foreach (var (mainHash, altHashes) in _mainToAltHash)
            {
                foreach (var altHash in altHashes)
                {
                    _altToMainHash[altHash] = mainHash;
                }
            }
        }

        public async Task<IReadOnlyList<TagInfo>> GetTagsForItemAsync(string fileHash)
        {
            List<TagInfo> LoadTags() => GetTagsForItem(fileHash)
                .Select(tag => new TagInfo {TagKind = TagKind.User, Tag = tag})
                .ToList();

            return await Task.Run(LoadTags);
        }

        public async Task StoreTagsForItemAsync(string fileHash, IReadOnlyList<string> tags)
        {
            var currentTagInfos = await GetTagsForItemAsync(fileHash);
            var currentTags = currentTagInfos.Select(tagInfo => tagInfo.Tag).ToHashSet();

            foreach (var tag in tags) currentTags.Add(tag);

            var orderedTags = currentTags.OrderBy(tag => tag).ToList();

            await Task.Run(() => SaveTagsForItem(fileHash, orderedTags));
        }

        public Task<string> GetMainHashForItemAsync(string fileHash)
        {
            var result = _altToMainHash.TryGetValue(fileHash, out var mainHash)
                ? mainHash
                : null;

            return Task.FromResult(result);
        }

        public async Task StoreAlternativeHash(string fileHash, string altHash)
        {
            if (!_mainToAltHash.TryGetValue(fileHash, out var altHashes))
            {
                altHashes = _mainToAltHash[fileHash] = new HashSet<string>();
            }

            altHashes.Add(altHash);

            await Task.Run(SaveHashMap);
        }
    }
}