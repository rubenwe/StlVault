using StlVault.AppModel.ViewModels;
using StlVault.Util.Collections;

namespace StlVault.Views
{
    internal class ItemsView : ContainerView<ItemsModel, ItemView, ItemModel>
    {
        protected override IReadOnlyObservableList<ItemModel> Items => ViewModel.Items;
    }
}