using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;
using StlVault.AppModel;
using StlVault.Config;
using StlVault.Util.FileSystem;

namespace StlVault.Tests
{
    public class ImportFolderTests
    {
        private Mock<IKnownItemStore> _store;
        private Mock<IFileSystem> _fileSystem;
        private Mock<ILibrary> _library;
        private ImportFolder _folder;

        private void Setup(
            ImportFolderConfig config,
            IReadOnlyList<KnownItemInfo> known,
            IEnumerable<IFileInfo> files)
        {
            _store = new Mock<IKnownItemStore>();
            _store.Setup(s => s.GetKnownItemsInLocationAsync(config.FullPath, true))
                .ReturnsAsync(known);

            _fileSystem = new Mock<IFileSystem> {DefaultValue = DefaultValue.Mock};
            _fileSystem.Setup(f => f.GetFiles(Constants.SupportedFilePattern, true))
                .Returns(files);

            _library = new Mock<ILibrary>();

            _folder = new ImportFolder(config, _fileSystem.Object, _store.Object, _library.Object);
        }

        [Test]
        public void NewFileShouldBeAddedToLibrary()
        {
            var config = new ImportFolderConfig {FullPath = @"C:\Users\User\3DObjects", ScanSubDirectories = true};
            var files = new[] {new KnownItemInfo {LastChange = DateTime.Now, RelativePath = "test.stl"}};
            
            Setup(config, known: null, files);

            TestUtils.Run(_folder.InitializeAsync);
            
            _library.Verify(l => l.ImportRangeAsync(config, It.Is<IReadOnlyCollection<string>>(c => c.Count == 1)));
            _library.Verify(l => l.RemoveRangeAsync(config, It.Is<IReadOnlyCollection<string>>(c => c.Count == 0)));
        }

        [Test]
        public void MissingFilesShouldBeRemovedFromLibrary()
        {
            var config = new ImportFolderConfig {FullPath = @"C:\Users\User\3DObjects", ScanSubDirectories = true};
            var known = new[] {new KnownItemInfo {LastChange = DateTime.Now, RelativePath = "test.stl"}};
            var files = Enumerable.Empty<IFileInfo>();
            
            Setup(config, known, files);

            TestUtils.Run(_folder.InitializeAsync);
            
            _library.Verify(l => l.ImportRangeAsync(config, It.Is<IReadOnlyCollection<string>>(c => c.Count == 0)));
            _library.Verify(l => l.RemoveRangeAsync(config, It.Is<IReadOnlyCollection<string>>(c => c.Count == 1)));
        }
    }
}