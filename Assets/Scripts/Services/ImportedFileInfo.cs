using System;

namespace StlVault.Services
{
    internal class ImportedFileInfo
    {
        public string SourceId { get; set; }
        public string FilePath { get; set; }
        public DateTime LastChange { get; set; }
    }
}