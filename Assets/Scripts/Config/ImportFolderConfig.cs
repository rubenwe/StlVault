using System.Collections.Generic;
using StlVault.Services;

namespace StlVault.Config
{
    internal class ImportFolderConfig : FileSourceConfig
    {
        public AutoTagMode AutoTagMode { get; set; }
        public List<string> AdditionalTags { get; set; }
        public string FullPath { get; set; }
        public bool ScanSubDirectories { get; set; }
    }
}