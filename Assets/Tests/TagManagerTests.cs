using System.Linq;
using NUnit.Framework;
using StlVault.Util.Collections;
using StlVault.Util.Tags;

namespace StlVault.Tests
{
    public class TagManagerTests
    {
        private class Tagged : ITagged
        {
            public ObservableSet<string> Tags { get; } = new ObservableSet<string>();
        }

        [Test]
        public void ShouldFindMatchingRecommendations()
        {
            var search = "test";
            var previous = new string[0];
            var expected = new[] {"test1", "test2"};

            var tagged = new[]
            {
                new Tagged {Tags = {"test1", "test2"}}
            };

            var tagManager = new TagManager();
            tagManager.AddFrom(tagged);

            var results = tagManager.GetRecommendations(tagged, previous, search);
            Assert.IsTrue(expected.SequenceEqual(results.Select(r => r.SearchTag)));
        }

        [Test]
        public void ShouldOrderRecommendationsBasedOnOccurence()
        {
            var search = "test";
            var previous = new string[0];
            var expected = new[] {"test2", "test1"};

            var tagged = new[]
            {
                new Tagged {Tags = {"test1", "test2"}},
                new Tagged {Tags = {"test2"}}
            };

            var tagManager = new TagManager();
            tagManager.AddFrom(tagged);

            var results = tagManager.GetRecommendations(tagged, previous, search);
            Assert.IsTrue(expected.SequenceEqual(results.Select(r => r.SearchTag)));
        }

        [Test]
        public void ShouldNotRecommendPreviouslySearchedTags()
        {
            var search = "test";
            var previous = new[] {"test1"};
            var expected = new[] {"test2"};

            var tagged = new[]
            {
                new Tagged {Tags = {"test1", "test2"}}
            };

            var tagManager = new TagManager();
            tagManager.AddFrom(tagged);

            var results = tagManager.GetRecommendations(tagged, previous, search);
            Assert.IsTrue(expected.SequenceEqual(results.Select(r => r.SearchTag)));
        }

        [Test]
        public void FilterShouldReturnMatchingModels()
        {
            var filters = new[]
            {
                "test1",
            };
            
            var tagged = new[]
            {
                new Tagged {Tags = {"test1"}},
                new Tagged {Tags = {"test1", "test2"}},
                new Tagged {Tags = {"test2"}},
            };

            var tagManager = new TagManager();
            tagManager.AddFrom(tagged);

            var results = tagManager.Filter(tagged, filters);
            
            Assert.True(results.Count == 2);
            Assert.True(results.Contains(tagged[0]));
            Assert.True(results.Contains(tagged[1]));
        }

        [Test]
        public void FilterShouldAllowEndingOnWildcards()
        {
            var filters = new[] { "test*" };
            
            var tagged = new[]
            {
                new Tagged {Tags = {"test1"}},
                new Tagged {Tags = {"test2"}},
                new Tagged {Tags = {"test1", "test2"}},
                new Tagged {Tags = {"tortoise"}},
            };

            var tagManager = new TagManager();
            tagManager.AddFrom(tagged);

            var results = tagManager.Filter(tagged, filters);
            
            Assert.True(results.Count == 3);
            Assert.False(results.Contains(tagged[3]));
        }
        
        [Test]
        public void FilterShouldAllowStartingOnNotSigns()
        {
            var filters = new[] { "-test*", "-dog", "- cat" };
            
            var tagged = new[]
            {
                new Tagged {Tags = {"test1"}},
                new Tagged {Tags = {"test1", "test2"}},
                new Tagged {Tags = {"dog"}},
                new Tagged {Tags = {"cat"}},
                new Tagged {Tags = {"tortoise"}},
            };

            var tagManager = new TagManager();
            tagManager.AddFrom(tagged);

            var results = tagManager.Filter(tagged, filters);
            
            Assert.True(results.Count == 1);
            Assert.True(results.Contains(tagged[4]));
        }

        [Test]
        public void RecommendationsShouldSupportNotSigns()
        {
            var search = "-test";
            var previous = new string[0];
            var expected = new[] {"-test1", "-test2"};

            var tagged = new[]
            {
                new Tagged {Tags = {"test1", "test2"}}
            };

            var tagManager = new TagManager();
            tagManager.AddFrom(tagged);

            var results = tagManager.GetRecommendations(tagged, previous, search);
            Assert.IsTrue(expected.SequenceEqual(results.Select(r => r.SearchTag)));
        }
    }
}