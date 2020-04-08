using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using StlVault.Messages;
using StlVault.Services;
using StlVault.Util.Logging;
using StlVault.Util.Messaging;
using StlVault.Util.Unity;
using StlVault.ViewModels;
using UnityEngine;
using UnityEngine.UI;

#pragma warning disable 0649

namespace StlVault.Views
{
    public class ViewInitializer : MonoBehaviour
    {
        [Category("Main Menu")] 
        [SerializeField] private ImportFoldersView _importFoldersView;
        [SerializeField] private SavedSearchesView _savedSearchesView;
        [SerializeField] private CollectionsView _collectionsView;

        [Category("Detail Menu")]
        [SerializeField] private DetailMenu _detailMenu;
        
        [Category("Main Area")]
        [SerializeField] private TagInputView _searchView;
        [SerializeField] private ItemsView _itemsView;

        [Category("Dialogs")] 
        [SerializeField] private AddSavedSearchDialog _addSavedSearchDialog;
        [SerializeField] private AddImportFolderDialog _addImportFolderDialog;
        [SerializeField] private ApplicationSettingsDialog _applicationSettingsDialog;
        [SerializeField] private ExitingDialog _exitingDialog;

        [Category("Misc")] 
        [SerializeField] private PreviewCam _previewBuilder;
        [SerializeField] private ApplicationView _applicationView;
        [SerializeField] private ProgressView _progressView;
        
        private readonly List<IDisposable> _disposables = new List<IDisposable>();
        private MessageAggregator _relay;
        private Library _library;

        private void Awake()
        {
            Texture.allowThreadedTextureCreation = true;
            Application.wantsToQuit += OnQuitRequested;
        }

        #if UNITY_EDITOR
        private async void OnDestroy()
        {
            await _library.StoreChangesAsync();
            Debug.Log("Saved");
        }
        #endif

        private bool OnQuitRequested()
        {
            Application.wantsToQuit -= OnQuitRequested;
            
            foreach (var disposable in _disposables)
            {
                disposable.Dispose();
            }
            
            GuiCallbackQueue.Enqueue(() => _relay.Send<RequestShowExitingDialogMessage>(this));
            
            return false;
        }
        
        private static void OnShutdownComplete() => Application.Quit();

        [SuppressMessage("ReSharper", "RedundantTypeArgumentsOfMethod")]
        private async void Start()
        {
            var aggregator = new MessageAggregator();

            _relay = aggregator;
            IConfigStore configStore = new AppDataConfigStore();
            IPreviewImageStore previewStore = new AppDataPreviewImageStore();
            
            _library = new Library(configStore, _previewBuilder, previewStore, _relay);
            var factory = new ImportFolderFactory(_library);

            // Main View
            var progressModel = new ProgressModel();
            var applicationModel = new ApplicationModel(_relay);
            var searchViewModel = new SearchModel(_library, _relay);
            var itemsViewModel = new ItemsModel(_library, _relay);

            // Main Menu
            var importFoldersViewModel = new ImportFoldersModel(configStore, factory, _relay);
            var savedSearchesViewModel = new SavedSearchesModel(configStore, _relay);
            var collectionsViewModel = new CollectionsModel(configStore, _relay);

            // Detail Menu
            var detailMenuModel = new DetailMenuModel(_library);

            // Dialogs
            var addSavedSearchViewModel = new AddSavedSearchModel(_relay);
            var addImportFolderViewModel = new AddImportFolderModel(_relay);
            var applicationSettingsModel = new ApplicationSettingsModel(configStore);
            var exitingModel = new ExitingModel(_library, OnShutdownComplete);
            
            BindViewModels();
            BindSettings();
            _disposables.Add(importFoldersViewModel);

            // Also restores app settings for import etc.
            await applicationSettingsModel.InitializeAsync();
            
            await _library.InitializeAsync();
            await InitializeViewModels();

            aggregator.Subscribe(
                // Main View
                progressModel,
                searchViewModel,
                itemsViewModel,
                
                // Main Menu
                importFoldersViewModel,
                savedSearchesViewModel,
                collectionsViewModel,
                
                // DetailMenu
                detailMenuModel,

                // Dialogs
                addSavedSearchViewModel,
                addImportFolderViewModel,
                applicationSettingsModel,
                exitingModel);

            void BindSettings()
            {
                var rt = applicationSettingsModel.RuntimeSettings;

                rt.ImportParallelism.ValueChanged += factor => _library.Parallelism = factor;
                rt.LogLevel.ValueChanged += logLevel => UnityLogger.LogLevel = logLevel;
                
                rt.UiScalePercent.ValueChanged += factor =>
                {
                    foreach (var canvas in FindObjectsOfType<Canvas>())
                    {
                        canvas.scaleFactor = applicationSettingsModel.UiScalePercent / 125f;
                    }
                };

                rt.ScrollSensitivity.ValueChanged += sensitivity =>
                {
                    foreach (var area in FindObjectsOfType<ScrollRect>())
                    {
                        area.scrollSensitivity = sensitivity;
                    }
                };
                
                rt.PreviewResolution.ValueChanged += res => _previewBuilder.PreviewResolution.Value = Mathf.RoundToInt(Mathf.Pow(2f, res));
                rt.PreviewJpegQuality.ValueChanged += quality => _previewBuilder.Quality = quality;
            }

            void BindViewModels()
            {
                // Main View
                _progressView.BindTo(progressModel);
                _applicationView.BindTo(applicationModel);
                _searchView.BindTo(searchViewModel);
                _itemsView.BindTo(itemsViewModel);

                // Main Menu
                _importFoldersView.BindTo(importFoldersViewModel);
                _savedSearchesView.BindTo(savedSearchesViewModel);
                _collectionsView.BindTo(collectionsViewModel);

                // Detail Menu
                _detailMenu.BindTo(detailMenuModel);
                
                // Dialogs
                _addImportFolderDialog.BindTo(addImportFolderViewModel);
                _addSavedSearchDialog.BindTo(addSavedSearchViewModel);
                _applicationSettingsDialog.BindTo(applicationSettingsModel);
                _exitingDialog.BindTo(exitingModel);
            }

            async Task InitializeViewModels()
            {
                await savedSearchesViewModel.InitializeAsync();
                await importFoldersViewModel.InitializeAsync();
                await collectionsViewModel.Initialize();
            }
        }
        
        
    }
}