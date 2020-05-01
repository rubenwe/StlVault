using System;
using System.Linq;
using SFB;
using StlVault.Util.Commands;
using StlVault.ViewModels;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

#pragma warning disable 0649

namespace StlVault.Views
{
    internal class AddImportFolderDialog : DialogView<AddImportFolderModel>
    {
        [Header("FolderAlias")] 
        [SerializeField] private TMP_InputField _aliasField;
        
        [Header("Folder Path")] 
        [SerializeField] private TMP_InputField _importPathField;
        [SerializeField] private Button _browseButton;
        [SerializeField] private Toggle _scanSubDirsToggle;

        [Header("Tags")] 
        [SerializeField] private TMP_InputField _tagsField;

        [Header("Auto Rotate")] 
        [SerializeField] private Toggle _rotateFilesToggle;
        [SerializeField] private TMP_InputField _xRotationField;
        [SerializeField] private TMP_InputField _yRotationField;
        [SerializeField] private TMP_InputField _zRotationField;

        [Header("Auto Scale")] 
        [SerializeField] private Toggle _scaleFilesToggle;
        [SerializeField] private TMP_InputField _xScaleField;
        [SerializeField] private TMP_InputField _yScaleField;
        [SerializeField] private TMP_InputField _zScaleField;

        protected override void OnViewModelBound()
        {
            base.OnViewModelBound();
            // Alias 
            _aliasField.BindTo(ViewModel.Alias);
            
            // Folder Path
            _importPathField.BindTo(ViewModel.FolderPath);
            _browseButton.onClick.AddListener(BrowseButtonClicked);
            _scanSubDirsToggle.BindTo(ViewModel.ScanSubDirectories);

            // Tags
            _tagsField.BindTo(ViewModel.Tags);

            // Auto Rotate
            _rotateFilesToggle.BindTo(ViewModel.RotateOnImport);
            (_xRotationField, _yRotationField, _zRotationField).BindTo(ViewModel.Rotation);

            // Auto Scale
            _scaleFilesToggle.BindTo(ViewModel.ScaleOnImport);
            (_xScaleField, _yScaleField, _zScaleField).BindTo(ViewModel.Scale);
        }

        private void BrowseButtonClicked()
        {
            var dir = ViewModel.AcceptCommand.CanExecute()
                ? ViewModel.FolderPath
                : Environment.GetFolderPath(Environment.SpecialFolder.MyComputer);

            var folder = StandaloneFileBrowser.OpenFolderPanel("Select new Import Folder", dir, false).FirstOrDefault();
            if (folder != null)
            {
                ViewModel.FolderPath.Value = folder;
            }
        }
    }
}