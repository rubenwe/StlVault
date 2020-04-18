using System.Collections.Generic;

namespace StlVault.Services
{
    public interface ITagIndex
    {
        IReadOnlyList<TagSearchResult> GetRecommendations(IEnumerable<string> currentFilters, string search);
    }
}