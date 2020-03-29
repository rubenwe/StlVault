using System.Linq;
using NUnit.Framework;
using StlVault.Util;

namespace StlVault.Tests
{
    public class ArrayTrieTests
    {
        [Test]
        public void ShouldFindStoredWord()
        {
            var trie = new ArrayTrie();
            
            trie.Insert("test");
            var results = trie.Find("te").Select(t => t.word).ToList();
            
            Assert.AreEqual(1, results.Count);
            Assert.Contains("test", results);
        }

        [Test]
        public void ShouldIncrementOccurrencesOnMultipleInserts()
        {
            var trie = new ArrayTrie();
            
            trie.Insert("test");
            trie.Insert("test");
            var results = trie.Find("te").ToList();
            
            Assert.AreEqual(1, results.Count);
            Assert.AreEqual(2, results[0].occurrences);
        }
    }
}