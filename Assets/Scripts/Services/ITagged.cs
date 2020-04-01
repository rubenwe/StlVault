using System.Collections.Generic;

namespace StlVault.Services
{
    internal interface ITagged
    {
        HashSet<string> Tags { get; }
    }
}