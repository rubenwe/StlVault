using StlVault.Util.Collections;
using StlVault.ViewModels;

#pragma warning disable 0649

namespace StlVault.Views
{
    internal class ItemSelector : ContainerView<ItemSelectorModel, SelectorItemView, ItemPreviewModel>
    {
        protected override IReadOnlyObservableList<ItemPreviewModel> ChildModels => ViewModel.Models;
        public ObservableList<ItemPreviewModel> Selection => ViewModel.Selected;

        protected override void OnChildViewInstantiated(SelectorItemView view)
        {
            view.Parent = this;
        }
    }
}