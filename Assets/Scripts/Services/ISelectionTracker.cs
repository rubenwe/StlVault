using System;
using StlVault.Config;
using StlVault.Util;
using StlVault.Util.Collections;

namespace StlVault.Services
{
    internal interface ISelectionTracker
    {
        BindableProperty<PreviewInfo> CurrentSelected { get; }
        IReadOnlyObservableCollection<PreviewInfo> Selection { get; }
        void SetSelected(PreviewInfo info);
        void SetDeselected(PreviewInfo info);
        bool IsSelected(PreviewInfo info);
        IDisposable EnterMassUpdate();
    }
}