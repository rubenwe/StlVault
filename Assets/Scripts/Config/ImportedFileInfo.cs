using System;
using Newtonsoft.Json;
using StlVault.Util.FileSystem;
using UnityEngine.Scripting;

namespace StlVault.Config
{
    internal class ImportedFileInfo
    {
        [JsonConstructor, Preserve]
        public ImportedFileInfo(string hash, DateTime lastChange)
        {
            Hash = hash;
            LastChange = lastChange;
        }

        public ImportedFileInfo(IFileInfo file, string hash)
        {
            Hash = hash;
            Path = file.Path;
            LastChange = file.LastChange;
        }

        [JsonIgnore]
        public string Path { get; set; }

        public string Hash { get; }
        public DateTime LastChange { get; }
    }
}