using System;
using System.Linq;
using SFB;
using StlVault.AppModel.ViewModels;
using StlVault.Util.Commands;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Button = UnityEngine.UI.Button;

#pragma warning disable 0649

namespace StlVault.Views
{
    internal class AddImportFolderDialog : DialogView<AddImportFolderModel>
    {
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
            
            // Folder Path
            _importPathField.Bind(ViewModel.FolderPath);
            _browseButton.onClick.AddListener(BrowseButtonClicked);
            _scanSubDirsToggle.Bind(ViewModel.ScanSubDirectories);
            
            // Tags
            _tagsField.Bind(ViewModel.Tags);
            
            // Auto Rotate
            _rotateFilesToggle.Bind(ViewModel.RotateOnImport);
            (_xRotationField, _yRotationField, _zRotationField).Bind(ViewModel.Rotation);
            
            // Auto Scale
            _scaleFilesToggle.Bind(ViewModel.ScaleOnImport);
            (_xScaleField, _yScaleField, _zScaleField).Bind(ViewModel.Scale);
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