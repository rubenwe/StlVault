using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using StlVault.Services;

namespace StlVault.Util.Tags
{
    internal class TagManager
    {
        [NotNull] private readonly ArrayTrie _trie = new ArrayTrie();

        public void Add(string tag) => _trie.Insert(tag);
        public void Add(IReadOnlyCollection<string> tags) => _trie.Insert(tags);
        public void AddFrom(IEnumerable<ITagged> tagged) => _trie.Insert(tagged.SelectMany(t => t.Tags).ToList());
        
        public IReadOnlyList<TagSearchResult> GetRecommendations(IEnumerable<ITagged> previewModels,
            IEnumerable<string> currentFilters,
            string search, RecommendationMode mode)
        {
            IEnumerable<TagSearchResult> Search()
            {
                if(string.IsNullOrWhiteSpace(search)) yield break;
                
                var currentSet = currentFilters.ToHashSet();

                var positiveSearch = search.TrimStart('-', ' ');
                var isNotSearch = search.StartsWith("-");
                
                var possibleTags = _trie.Find(positiveSearch)
                    .Select(result => result.word)
                    .Where(tag => !currentSet.Contains(tag));
                
                var currentModels = Filter(previewModels, currentSet);

                foreach (var tag in possibleTags)
                {
                    if (mode == RecommendationMode.Tagging || currentModels.Count(model => model.Tags.Contains(tag)) > 0)
                    {
                        var rebuiltTag = isNotSearch ? "-" + tag : tag;
                        yield return new TagSearchResult(rebuiltTag, currentModels.Count(model => model.Tags.Contains(tag)));
                    }
                }
            }

            return Search()
                .OrderByDescending(result => result.MatchingItemCount)
                .ToList();
        }

        
        public IReadOnlyCollection<T> Filter<T>(IEnumerable<T> items, IReadOnlyCollection<string> filters) 
            where T : ITagged
        {
            var filter = FilterFactory.Create(filters);
            return items.Where(item => filter.Matches(item)).ToList();
        }
    }
}