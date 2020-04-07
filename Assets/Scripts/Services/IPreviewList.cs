using System;
using StlVault.Config;
using StlVault.Util.Collections;
using StlVault.ViewModels;

namespace StlVault.Services
{
    internal interface IPreviewList : IReadOnlyObservableList<ItemPreviewModel>, IDisposable
    {
        
    }
}