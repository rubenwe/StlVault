using System.Collections.Generic;

namespace StlVault.Config
{
    public class ImportFoldersConfig : List<ImportFolderConfig>
    {
        public ImportFoldersConfig()
        {
        }

        public ImportFoldersConfig(IEnumerable<ImportFolderConfig> collection) : base(collection)
        {
        }
    }
}