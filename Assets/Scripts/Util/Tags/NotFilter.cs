using System;
using JetBrains.Annotations;

namespace StlVault.Util.Tags
{
    internal class NotFilter : IFilter
    {
        private readonly IFilter _inner;

        public NotFilter([NotNull] IFilter inner)
        {
            _inner = inner ?? throw new ArgumentNullException(nameof(inner));
        }

        public bool Matches(ITagged tagged)
        {
            return !_inner.Matches(tagged);
        }
    }
}