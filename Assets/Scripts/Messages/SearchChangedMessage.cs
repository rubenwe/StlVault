using System.Collections.Generic;

namespace StlVault.Messages
{
    internal class SearchChangedMessage
    {
        public IReadOnlyList<string> SearchTags { get; set; }
    }
}