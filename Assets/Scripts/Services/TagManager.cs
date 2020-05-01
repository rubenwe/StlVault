using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using StlVault.Util;
using StlVault.Util.Collections;

namespace StlVault.Services
{
    internal class TagManager
    {
        [NotNull] private readonly ArrayTrie _trie = new ArrayTrie();

        public void Add(string tag) => _trie.Insert(tag);
        public void Add(IReadOnlyCollection<string> tags) => _trie.Insert(tags);
        public void AddFrom(ITagged tagged) => _trie.Insert(tagged.Tags);
        public void AddFrom(IEnumerable<ITagged> tagged) => _trie.Insert(tagged.SelectMany(t => t.Tags).ToList());
        
        public IReadOnlyList<TagSearchResult> GetRecommendations(
            IEnumerable<ITagged> previewModels, 
            IEnumerable<string> currentFilters, 
            string search)
        {
            IEnumerable<TagSearchResult> Search()
            {
                var currentSet = currentFilters.ToHashSet();
                
                var possibleTags = _trie.Find(search)
                    .Select(result => result.word)
                    .Where(tag => !currentSet.Contains(tag));
                
                var currentModels = GetMatching(previewModels, currentSet);

                foreach (var tag in possibleTags)
                {
                    var matching = currentModels.Count(model => model.Tags.Contains(tag));
                    if(matching > 0) yield return new TagSearchResult(tag, matching);
                }
            }

            return Search()
                .OrderByDescending(result => result.MatchingItemCount)
                .ToList();
        }

        private static IReadOnlyList<ITagged> GetMatching(
            IEnumerable<ITagged> models, 
            IReadOnlyCollection<string> filters)
        {
            return models.Where(model => filters.All(model.Tags.Contains)).ToList();
        }

        public IReadOnlyCollection<T> Filter<T>(
            IReadOnlyCollection<T> items, 
            IReadOnlyList<string> filters) 
            where T : ITagged
        {
            return items.Where(item => filters.All(item.Tags.Contains)).ToList();
        }
    }

    internal interface ITagged
    {
        ObservableSet<string> Tags { get; }
    }
}