using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace StlVault.Util.Tags
{
    internal static class FilterFactory
    {
        public static IFilter Create([NotNull] IReadOnlyCollection<string> filterStrings)
        {
            if (filterStrings == null) throw new ArgumentNullException(nameof(filterStrings));
            if (!filterStrings.Any()) return new AlwaysTrueFilter();

            IEnumerable<IFilter> Build()
            {
                foreach (var filterString in filterStrings)
                {
                    var search = filterString;
                    
                    if (search.StartsWith("-"))
                    {
                        search = search.Trim('-', ' ');
                    }
                    
                    var inner = BuildInner(search);
                    
                    yield return search != filterString 
                        ? new NotFilter(inner) 
                        : inner;
                }
            }

            return new AndCombinedFilter(Build().ToList());
        }

        private static IFilter BuildInner(string search)
        {
            if (search.EndsWith("*")) return new EndsInWildcardFilter(search);
            else return new AbsoluteMatchFilter(search);
        }
    }
}