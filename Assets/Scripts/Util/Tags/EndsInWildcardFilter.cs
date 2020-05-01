using System;
using System.Linq;
using JetBrains.Annotations;

namespace StlVault.Util.Tags
{
    internal class EndsInWildcardFilter : IFilter
    {
        private readonly string _search;

        public EndsInWildcardFilter([NotNull] string search)
        {
            if (search == null) throw new ArgumentNullException(nameof(search));
            _search = search.TrimEnd('*');
        }

        public bool Matches(ITagged tagged)
        {
            return tagged.Tags.Any(tag => tag.StartsWith(_search));
        }
    }
}