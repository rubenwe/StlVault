using System.Collections.Generic;

namespace StlVault.Config
{
    internal class SavedSearchesConfig : List<SavedSearchConfig>
    {
        public SavedSearchesConfig()
        {
        }

        public SavedSearchesConfig(IEnumerable<SavedSearchConfig> collection) : base(collection)
        {
        }
    }
}