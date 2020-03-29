using System;
using StlVault.Util.FileSystem;

namespace StlVault.AppModel
{
    internal class KnownItemInfo : IFileInfo
    {
        public KnownItemInfo(IFileInfo file)
        {
            RelativePath = file.RelativePath;
            LastChange = file.LastChange;
        }

        public KnownItemInfo()
        {
        }

        public string RelativePath { get; set; }
        
        public DateTime LastChange { get; set; }
    }
}