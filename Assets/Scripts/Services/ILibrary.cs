using System.Collections.Generic;


namespace StlVault.Services
{
    internal interface ILibrary
    {
        IPreviewList GetItemPreviewMetadata(IReadOnlyList<string> filters);
    }
}