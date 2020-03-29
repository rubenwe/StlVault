using System;
using JetBrains.Annotations;
using StlVault.Config;
using StlVault.Util.FileSystem;

namespace StlVault.Services
{
    internal class ImportFolderFactory : IImportFolderFactory
    {
        [NotNull] private readonly IKnownItemStore _store;
        [NotNull] private readonly ILibrary _library;

        public ImportFolderFactory([NotNull] IKnownItemStore store, [NotNull] ILibrary library)
        {
            _store = store ?? throw new ArgumentNullException(nameof(store));
            _library = library ?? throw new ArgumentNullException(nameof(library));
        }

        public ImportFolder Create(ImportFolderConfig folderConfig)
        {
            return new ImportFolder(folderConfig, new FolderFileSystem(folderConfig.FullPath), _store, _library);
        }
    }
}