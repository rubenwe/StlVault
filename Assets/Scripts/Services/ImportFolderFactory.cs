using System;
using JetBrains.Annotations;
using StlVault.Config;
using StlVault.Util.FileSystem;

namespace StlVault.Services
{
    internal class ImportFolderFactory : IImportFolderFactory
    {
        [NotNull] private readonly IFileSourceSubscriber _subscriber;

        public ImportFolderFactory([NotNull] IFileSourceSubscriber subscriber)
        {
            _subscriber = subscriber ?? throw new ArgumentNullException(nameof(subscriber));
        }

        public IImportFolder Create(ImportFolderConfig folderConfig)
        {
            var folder = new ImportFolder(folderConfig, new FolderFileSystem(folderConfig.FullPath));
            folder.Subscribe(_subscriber);
            
            return folder;
        }
    }
}