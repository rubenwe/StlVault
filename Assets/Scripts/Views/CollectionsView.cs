using StlVault.AppModel.ViewModels;
using StlVault.Util.Collections;

namespace StlVault.Views
{
    internal class CollectionsView : ContainerView<CollectionsModel, CollectionView, CollectionModel>
    {
        protected override IReadOnlyObservableList<CollectionModel> Items => ViewModel.Collections;
    }
}