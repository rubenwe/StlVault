using System.Collections.Generic;
using System.Linq;

namespace StlVault.Services
{
    public interface ITagIndex
    {
        IReadOnlyList<TagSearchResult> GetRecommendations(IEnumerable<string> currentFilters, string search);
    }
}