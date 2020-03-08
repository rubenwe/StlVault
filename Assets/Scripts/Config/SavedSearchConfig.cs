using System.Collections.Generic;

namespace StlVault.Config
{
    internal class SavedSearchConfig
    {
        public string Alias { get; set; }
        public List<string> Tags { get; set; } = new List<string>();
    }
}