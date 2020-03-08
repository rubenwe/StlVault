using System.Collections.Generic;
using System.Linq;

namespace StlVault.AppModel
{
    internal static class TaggedItemExtensions
    {
        internal static IReadOnlyList<T> Matching<T>(this ICollection<T> items, IEnumerable<string> filter)
            where T : ITagged
        {
            var lowerFilter = filter.Select(f => f.ToLowerInvariant()).ToList();
            return items.AsParallel()
                .Where(item => lowerFilter.All(item.Tags.Contains))
                .ToList();
        }
    }
}