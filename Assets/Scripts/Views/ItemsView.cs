using StlVault.Util.Collections;
using StlVault.ViewModels;

namespace StlVault.Views
{
    internal class ItemsView : ContainerView<ItemsModel, ItemView, FilePreviewModel>
    {
        protected override IReadOnlyObservableList<FilePreviewModel> Items => ViewModel.Items;
    }
}