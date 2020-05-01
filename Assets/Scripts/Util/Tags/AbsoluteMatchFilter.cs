using System;
using JetBrains.Annotations;

namespace StlVault.Util.Tags
{
    internal class AbsoluteMatchFilter : IFilter
    {
        private readonly string _tag;

        public AbsoluteMatchFilter([NotNull] string tag)
        {
            _tag = tag ?? throw new ArgumentNullException(nameof(tag));
        }
        
        public bool Matches(ITagged tagged) => tagged.Tags.Contains(_tag);
    }
}