using System.Collections.Generic;

// ReSharper disable CheckNamespace

namespace StlVault.AppModel.Messages
{
    internal class SearchChangedMessage
    {
        public IReadOnlyList<string> SearchTags { get; set; }
    }
}