using StlVault.AppModel;

namespace StlVault.Views
{
    internal class ImportFolderManager
    {
        private readonly IConfigStore _store;

        public ImportFolderManager(IConfigStore store)
        {
            _store = store;
        }
    }
}