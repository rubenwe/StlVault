using System.Collections.Generic;
using System.Linq;

namespace StlVault.AppModel
{
    public interface ITagIndex
    {
        IOrderedEnumerable<TagSearchResult> GetRecommendations(IEnumerable<string> currentFilters, string search);
    }
}