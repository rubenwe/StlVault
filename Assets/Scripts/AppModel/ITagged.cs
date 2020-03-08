using System.Collections.Generic;

namespace StlVault.AppModel
{
    internal interface ITagged
    {
        HashSet<string> Tags { get; }
    }
}