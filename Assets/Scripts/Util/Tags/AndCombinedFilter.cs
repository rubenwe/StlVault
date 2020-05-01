using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace StlVault.Util.Tags
{
    internal class AndCombinedFilter : IFilter
    {
        private readonly IReadOnlyList<IFilter> _filters;

        public AndCombinedFilter([NotNull] IReadOnlyList<IFilter> filters)
        {
            _filters = filters ?? throw new ArgumentNullException(nameof(filters));
        }

        public bool Matches(ITagged tagged)
        {
            return _filters.All(f => f.Matches(tagged));
        }
    }
}