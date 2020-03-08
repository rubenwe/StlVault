using System.Collections.Generic;

namespace StlVault.AppModel.Messages
{
    internal class SaveSearchMessage
    {
        public string Alias { get; set; }
        public IReadOnlyList<string> SearchTags { get; set; }
    }
}