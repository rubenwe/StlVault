using StlVault.Util.Collections;
using StlVault.ViewModels;
using UnityEngine;
using UnityEngine.UI;

#pragma warning disable 0649

namespace StlVault.Views
{
    internal class ImportFoldersView : ContainerView<ImportFoldersModel, ImportFolderView, FileSourceModel>
    {
        [SerializeField] private Button _addButton;
        protected override IReadOnlyObservableList<FileSourceModel> Items => ViewModel.Folders;

        protected override void OnViewModelBound()
        {
            base.OnViewModelBound();

            _addButton.Bind(ViewModel.AddImportFolderCommand);
        }
    }
}