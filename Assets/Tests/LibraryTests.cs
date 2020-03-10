using Moq;
using NUnit.Framework;
using StlVault.Util.FileSystem;

namespace StlVault.Tests
{
    public class LibraryTests
    {
        // A Test behaves as an ordinary method
        [Test]
        public void NewItemShouldBeAddedToLibrary()
        {
            var fs = Mock.Of<FileSystem>();
        }
    }

    
}
