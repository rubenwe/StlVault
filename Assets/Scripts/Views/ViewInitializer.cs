using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using StlVault.Config;
using StlVault.Messages;
using StlVault.Services;
using StlVault.Util.Logging;
using StlVault.Util.Messaging;
using StlVault.Util.Unity;
using StlVault.ViewModels;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;

#pragma warning disable 0649

namespace StlVault.Views
{
    public class ViewInitializer : MonoBehaviour
    {
        [Header("Main Menu")] 
        [SerializeField] private ImportFoldersView _importFoldersView;
        [SerializeField] private SavedSearchesView _savedSearchesView;
        [SerializeField] private CollectionsView _collectionsView;

        [Header("Detail Menu")]
        [SerializeField] private DetailMenu _detailMenu;
        
        [Header("Main Area")]
        [SerializeField] private TagInputView _searchView;
        [SerializeField] private CanvasGroup _mainGroup;
        [SerializeField] private LibraryView _libraryView;
        
        [Header("Dialogs")] 
        [SerializeField] private AddCollectionDialog _addCollectionDialog;
        [SerializeField] private AddSavedSearchDialog _addSavedSearchDialog;
        [SerializeField] private AddImportFolderDialog _addImportFolderDialog;
        [SerializeField] private ApplicationSettingsDialog _applicationSettingsDialog;
        [SerializeField] private UserFeedbackDialog _userFeedbackDialog;
        [SerializeField] private UpdateNotificationDialog _updateNotificationDialog;
        [SerializeField] private ExitingDialog _exitingDialog;
        [SerializeField] private EditScreen _editScreen;

        [Header("Misc")] 
        [SerializeField] private PreviewCam _previewBuilder;
        [SerializeField] private ApplicationView _applicationView;
        [SerializeField] private ProgressView _progressView;
        [SerializeField] private AppVersionButton _versionButton;
        
        private readonly List<IDisposable> _disposables = new List<IDisposable>();
        private MessageAggregator _relay;
        private Library _library;
        private IConfigStore _configStore;
        
        // Must be field or GC will reclaim it
        private SelectionTracker _tracker;

        private void Awake()
        {
            Texture.allowThreadedTextureCreation = true;
            Application.wantsToQuit += OnQuitRequested;

            Migrator.Run();

            _configStore = new ConfigStore(Application.persistentDataPath);
            
            // Restore layout
            var layout = _configStore.LoadAsyncOrDefault<LayoutSettings>().Result;
            ApplyLayout(layout);

        }

        #if UNITY_EDITOR
        private async void OnDestroy()
        {
            var layout = GatherLayoutSettings();
            await _configStore.StoreAsync(layout);
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
            
            GuiCallbackQueue.Enqueue(async () =>
            {
                var layout = GatherLayoutSettings();
                await _configStore.StoreAsync(layout);
            });
            
            GuiCallbackQueue.Enqueue(() => _relay.Send<RequestShowDialogMessage.ExitingDialog>(this));
            
            return false;
        }
        
        private static void OnShutdownComplete() => Application.Quit();

        [SuppressMessage("ReSharper", "RedundantTypeArgumentsOfMethod")]
        private async void Start()
        {
            var aggregator = new MessageAggregator();

            _relay = aggregator;
            IPreviewImageStore previewStore = new PreviewImageStore(Application.persistentDataPath);
            
            _library = new Library(_configStore, _previewBuilder, previewStore, _relay);
            var factory = new ImportFolderFactory(_library);
            
            // Misc
            var checker = new UpdateChecker(_relay);
            var appVersionModel = new AppVersionDisplayModel(_relay);
            _tracker = new SelectionTracker(_library);
            
            // Main View
            var progressModel = new ProgressModel();
            var applicationModel = new ApplicationModel(_relay);
            var searchViewModel = new SearchModel(_library, _relay);
            var itemsViewModel = new ItemsModel(_library, _relay);

            // Main Menu
            var importFoldersViewModel = new ImportFoldersModel(_configStore, factory, _relay);
            var savedSearchesViewModel = new SavedSearchesModel(_configStore, _relay);
            var collectionsViewModel = new CollectionsModel(_configStore, _library, _relay);

            // Detail Menu
            var detailMenuModel = new DetailMenuModel(_library, _relay);

            // Dialogs
            var addCollectionModel = new AddCollectionModel(_relay);
            var addSavedSearchViewModel = new AddSavedSearchModel(_relay);
            var addImportFolderViewModel = new AddImportFolderModel(_relay);
            var applicationSettingsModel = new ApplicationSettingsModel(_configStore);
            var userFeedbackModel = new UserFeedbackModel();
            var updateNotificationModel = new UpdateNotificationModel(_configStore);
            var exitingModel = new ExitingModel(_library, OnShutdownComplete);
            var editScreenModel = new EditScreenModel(_library);
            
            BindViewModels();
            BindSettings();
            
            // Wire up misc items
            _disposables.Add(importFoldersViewModel);
            _editScreen.MainView = _mainGroup;

            // Also restores app settings for import etc.
            await applicationSettingsModel.InitializeAsync();
            
            await _library.InitializeAsync();
            InitializeViewModelsAsync();

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

                // Misc
                appVersionModel,
                _tracker,
                
                // Dialogs
                addCollectionModel,
                addSavedSearchViewModel,
                addImportFolderViewModel,
                applicationSettingsModel,
                userFeedbackModel,
                updateNotificationModel,
                exitingModel,
                editScreenModel);

            void BindViewModels()
            {
                // Main View
                _progressView.BindTo(progressModel);
                _applicationView.BindTo(applicationModel);
                _searchView.BindTo(searchViewModel);
                _libraryView.BindTo(itemsViewModel);

                // Main Menu
                _importFoldersView.BindTo(importFoldersViewModel);
                _savedSearchesView.BindTo(savedSearchesViewModel);
                _collectionsView.BindTo(collectionsViewModel);

                // Detail Menu
                _detailMenu.BindTo(detailMenuModel);
                
                // Misc
                _versionButton.BindTo(appVersionModel);
                
                // Dialogs
                _addCollectionDialog.BindTo(addCollectionModel);
                _addImportFolderDialog.BindTo(addImportFolderViewModel);
                _addSavedSearchDialog.BindTo(addSavedSearchViewModel);
                _applicationSettingsDialog.BindTo(applicationSettingsModel);
                _userFeedbackDialog.BindTo(userFeedbackModel);
                _updateNotificationDialog.BindTo(updateNotificationModel);
                _exitingDialog.BindTo(exitingModel);
                _editScreen.BindTo(editScreenModel);
            }
            
            void BindSettings()
            {
                var rt = applicationSettingsModel.RuntimeSettings;

                rt.ImportParallelism.ValueChanged += factor => _library.Parallelism = factor;
                rt.LogLevel.ValueChanged += logLevel => UnityLogger.LogLevel = logLevel;
                
                rt.UiScalePercent.ValueChanged += factor =>
                {
                    foreach (var canvas in FindObjectsOfTypeAll<CanvasScaler>())
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

            async void InitializeViewModelsAsync()
            {
                await savedSearchesViewModel.InitializeAsync();
                await importFoldersViewModel.InitializeAsync();
                await collectionsViewModel.InitializeAsync();
                await updateNotificationModel.InitializeAsync();
                
                // run update checks
                await checker.CheckForUpdatesAsync();
            }
        }

        private void ApplyLayout(LayoutSettings layout)
        {
            _importFoldersView.Expanded = layout.MainMenu.ImportFoldersExpanded;
            _savedSearchesView.Expanded = layout.MainMenu.SavedSearchesExpanded;
            _collectionsView.Expanded = layout.MainMenu.CollectionsExpanded;
        }

        private LayoutSettings GatherLayoutSettings()
        {
            return new LayoutSettings
            {
                MainMenu =
                {
                    ImportFoldersExpanded = _importFoldersView.Expanded,
                    SavedSearchesExpanded = _savedSearchesView.Expanded,
                    CollectionsExpanded = _collectionsView.Expanded,
                }
            };
        }

        private static List<T> FindObjectsOfTypeAll<T>()
        {
            return SceneManager.GetActiveScene().GetRootGameObjects()
                .SelectMany(g => g.GetComponentsInChildren<T>(true))
                .ToList();
        }
    }
}