using System.Collections.Generic;

namespace StlVault.Messages
{
    internal class RequestShowSaveSearchDialogMessage
    {
        public IReadOnlyList<string> SearchTags { get; set; }
    }
}