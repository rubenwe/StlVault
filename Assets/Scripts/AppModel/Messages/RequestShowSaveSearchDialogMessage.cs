using System.Collections.Generic;

namespace StlVault.AppModel.Messages
{
    internal class RequestShowSaveSearchDialogMessage
    {
        public IReadOnlyList<string> SearchTags { get; set; }
    }
}