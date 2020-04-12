using StlVault.Util.Collections;
using StlVault.ViewModels;
using UnityEngine;
using UnityEngine.UI;

#pragma warning disable 0649

namespace StlVault.Views
{
    internal class SavedSearchesView : ContainerView<SavedSearchesModel, SavedSearchView, SavedSearchModel>
    {
        [SerializeField] private Button _addButton;

        protected override IReadOnlyObservableList<SavedSearchModel> ChildModels => ViewModel.SavedSearches;

        protected override void OnViewModelBound()
        {
            base.OnViewModelBound();

            _addButton.BindTo(ViewModel.SaveCurrentSearchCommand);
        }
    }
}