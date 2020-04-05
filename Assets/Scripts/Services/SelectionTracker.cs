using StlVault.Config;
using StlVault.Util;
using StlVault.Util.Collections;

namespace StlVault.Services
{
    internal class SelectionTracker : ISelectionTracker
    {
        private readonly ObservableSet<PreviewInfo> _selection = new ObservableSet<PreviewInfo>();
        
        public BindableProperty<PreviewInfo> CurrentSelected { get; } = new BindableProperty<PreviewInfo>();
        public IReadOnlyObservableCollection<PreviewInfo> Selection => _selection;

        public void SetSelected(PreviewInfo info)
        {
            _selection.Add(info);
            CurrentSelected.Value = info;
        }

        public void SetDeselected(PreviewInfo info)
        {
            _selection.Remove(info);
            CurrentSelected.Value = null;
        }

        public bool IsSelected(PreviewInfo info) => _selection.Contains(info);
    }
}