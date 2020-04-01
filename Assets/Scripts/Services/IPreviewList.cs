using System;
using StlVault.Config;
using StlVault.Util.Collections;

namespace StlVault.Services
{
    internal interface IPreviewList : IReadOnlyObservableList<PreviewInfo>, IDisposable
    {
        
    }
}