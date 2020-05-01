using StlVault.Util.Collections;

namespace StlVault.Util.Tags
{
    internal interface ITagged
    {
        ObservableSet<string> Tags { get; }
    }
}