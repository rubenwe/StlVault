using System.Diagnostics.CodeAnalysis;
using StlVault.Util;
using UnityEngine.EventSystems;

namespace StlVault.Views
{
    internal class SelectorItemView : ItemViewBase, IPointerClickHandler
    {
        public BindableProperty<bool> IsSelected { get; } = new BindableProperty<bool>();
        public ItemSelector Parent { private get; set; }

        protected override void OnViewModelBound()
        {
            IsSelected.Value = Parent.Selection.Contains(ViewModel);
            
            ViewModel.PreviewChanged += PreviewChanged;
            
            IsSelected.ValueChanged += OnIsSelectedChanged;
            SelectedChanged(IsSelected);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            IsSelected.Value = !IsSelected.Value;
        }

        [SuppressMessage("ReSharper", "Unity.NoNullPropagation")]
        private void OnIsSelectedChanged(bool selected)
        {
            if (selected) Parent.Selection.Add(ViewModel);
            else Parent.Selection.Remove(ViewModel);

            SelectedChanged(selected);
        }
    }
}