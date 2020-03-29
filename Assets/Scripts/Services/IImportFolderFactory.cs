using StlVault.Config;

namespace StlVault.Services
{
    internal interface IImportFolderFactory
    {
        ImportFolder Create(ImportFolderConfig folderConfig);
    }
}