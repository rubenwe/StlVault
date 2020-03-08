using System.Collections.Generic;

namespace StlVault.AppModel
{
    internal interface ILibrary
    {
        IReadOnlyList<ItemPreviewMetadata> GetItemPreviewMetadata(IReadOnlyList<string> filters);
    }
}