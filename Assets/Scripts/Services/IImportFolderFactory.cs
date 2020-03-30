using StlVault.Config;

namespace StlVault.Services
{
    internal interface IImportFolderFactory
    {
        IImportFolder Create(ImportFolderConfig folderConfig);
    }
}