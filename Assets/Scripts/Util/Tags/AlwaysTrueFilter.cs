namespace StlVault.Util.Tags
{
    internal class AlwaysTrueFilter : IFilter
    {
        public bool Matches(ITagged tagged) => true;
    }
}