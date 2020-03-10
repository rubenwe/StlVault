using System.Collections.Generic;

namespace StlVault.Config
{
    public class ImportFolderConfig
    {
        public string FullPath { get; set; }
        public bool ScanSubDirectories { get; set; }
        
        public AutoTagMode AutoTagMode { get; set; }
        public List<string> Tags { get; set; }
        public ConfigVector3? Scale { get; set; }
        public ConfigVector3? Rotation { get; set; }
    }
}