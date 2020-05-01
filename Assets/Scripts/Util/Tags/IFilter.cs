namespace StlVault.Util.Tags
{
    internal interface IFilter
    {
        bool Matches(ITagged tagged);
    }
}