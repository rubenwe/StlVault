using Moq;
using NUnit.Framework;
using StlVault.Util.FileSystem;

namespace StlVault.Tests
{
    public class LibraryTests
    {
        [Test]
        public void NewFileShouldBeAddedToLibrary()
        {
            var fs = Mock.Of<FileSystem>();
        }

        [Test]
        public void NewFolderShouldBeAddedToLibrary()
        {
        }

        [Test]
        public void MovedFileShouldBeRemovedFromOldFolder()
        {
        }

        [Test]
        public void DuplicateFileInSameFolderShouldBeDeduplicated()
        {
        }

        [Test]
        public void DuplicateFileInOtherFolderShouldBeListedForBothFolders()
        {
        }

        [Test]
        public void DeletingFileShouldRemoveFromLibrary()
        {
        }

        [Test]
        public void PreviewImagesOfDeletedFilesShouldBeCleanedUp()
        {
        }

        [Test]
        public void AddingANewImportFolderShouldTriggerReimport()
        {
        }

        [Test]
        public void MovedItemsShouldRebuildPreviewsIfRotationChanged()
        {
        }

        [Test]
        public void MovedItemsShouldNotRebuildPreviewsIfRotationsIsTheSame()
        {
        }

        [Test]
        public void TagsFromParentFolderShouldBeAppliedToImportedItems()
        {
        }

        [Test]
        public void ItemShouldBeTaggedWithImportFolder()
        {
        }

        [Test]
        public void MovingItemsShouldRemoveInheritedTags()
        {
        }

        [Test]
        public void MovingItemShouldNotRemoveUserTags()
        {
        }
    }
}
