using System.Collections.Generic;

namespace StlVault.Config
{
    public class ImportFoldersConfigFile : List<ImportFolderConfig>
    {
        public ImportFoldersConfigFile()
        {
        }

        public ImportFoldersConfigFile(IEnumerable<ImportFolderConfig> collection) : base(collection)
        {
        }
    }
}