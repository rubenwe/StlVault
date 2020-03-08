using StlVault.AppModel.ViewModels;
using StlVault.Util.Collections;
using UnityEngine;
using UnityEngine.UI;

#pragma warning disable 0649

namespace StlVault.Views
{
    internal class ImportFoldersView : ContainerView<ImportFoldersModel, ImportFolderView, ImportFolderModel>
    {
        [SerializeField] private Button _addButton;
        protected override IReadOnlyObservableList<ImportFolderModel> Items => ViewModel.Folders;

        protected override void OnViewModelBound()
        {
            base.OnViewModelBound();
            
            _addButton.Bind(ViewModel.AddImportFolderCommand);
        }
    }
}