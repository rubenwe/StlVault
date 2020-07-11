using StlVault.Util.Collections;
using StlVault.ViewModels;
using UnityEngine;
using UnityEngine.UI;

#pragma warning disable 0649

namespace StlVault.Views
{
    internal class CollectionsView : MainMenuView<CollectionsModel, CollectionView, CollectionModel>
    {
        protected override IReadOnlyObservableList<CollectionModel> ChildModels => ViewModel.Collections;

        [SerializeField] private Button _addButton;

        protected override void OnViewModelBound()
        {
            base.OnViewModelBound();
            
            _addButton.BindTo(ViewModel.AddCollectionCommand);
        }
    }
}