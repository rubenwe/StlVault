using System;
using StlVault.Util.Collections;

namespace StlVault.Services
{
    internal interface IPreviewList : IReadOnlyObservableList<ItemPreviewMetadata>, IDisposable
    {
        
    }
}