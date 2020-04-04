using System.Collections.Generic;
using StlVault.Config;
using StlVault.Util;
using StlVault.Util.Collections;


namespace StlVault.Services
{
    internal interface ILibrary
    {
        IPreviewList GetItemPreviewMetadata(IReadOnlyList<string> filters);
        BindableProperty<PreviewInfo> CurrentSelected { get; }
        IReadOnlyObservableList<PreviewInfo> Selection { get; }
    }
}