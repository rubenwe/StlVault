using StlVault.Util.Collections;
using StlVault.ViewModels;

namespace StlVault.Views
{
    internal class CollectionsView : ContainerView<CollectionsModel, CollectionView, CollectionModel>
    {
        protected override IReadOnlyObservableList<CollectionModel> Items => ViewModel.Collections;
    }
}