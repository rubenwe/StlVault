using System.Collections.Generic;

namespace StlVault.Config
{
    internal class SavedSearchesConfigFile : List<SavedSearchConfig>
    {
        public SavedSearchesConfigFile()
        {
        }

        public SavedSearchesConfigFile(IEnumerable<SavedSearchConfig> collection) : base(collection)
        {
        }
    }
}