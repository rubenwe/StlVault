using System.Collections.Generic;

namespace StlVault.Config
{
    internal class ImportFolderConfig : FileSourceConfig
    {
        public string Alias { get; set; }
        public AutoTagMode AutoTagMode { get; set; }
        public List<string> AdditionalTags { get; set; }
        public string FullPath { get; set; }
        public bool ScanSubDirectories { get; set; }
    }
}