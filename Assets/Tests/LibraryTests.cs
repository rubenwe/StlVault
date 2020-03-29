using System.IO;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using NUnit.Framework;
using StlVault.Util.Messaging;

namespace StlVault.Tests
{
    public class LibraryTests
    {
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
