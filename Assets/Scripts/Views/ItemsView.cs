using StlVault.Util.Collections;
using StlVault.ViewModels;

namespace StlVault.Views
{
    internal class ItemsView : ContainerView<ItemsModel, ItemView, ItemModel>
    {
        protected override IReadOnlyObservableList<ItemModel> Items => ViewModel.Items;
    }
}