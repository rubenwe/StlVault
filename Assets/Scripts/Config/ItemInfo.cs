using System;
using System.Collections.Generic;

namespace StlVault.Config
{
    internal class ItemInfo
    {
        public string FileLocation { get; set; }
        public string FileHash { get; set; }
        public DateTime LastChange { get; set; }
     
        public string PreviewImageLocation { get; set; }
        
        public ConfigVector3 Rotation { get; set; }
        public ConfigVector3 Scale { get; set; }
        
        public List<(DataOrigin, string)> Tags { get; set; }
        public Dictionary<string, string> CustomMetadata { get; set; }
    }
}