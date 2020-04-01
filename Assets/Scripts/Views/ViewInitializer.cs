using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using StlVault.Services;
using StlVault.Util.Logging;
using StlVault.Util.Messaging;
using StlVault.ViewModels;
using UnityEngine;

#pragma warning disable 0649

namespace StlVault.Views
{
    public class ViewInitializer : MonoBehaviour
    {
        [Category("Main Menu")] 
        [SerializeField] private ImportFoldersView _importFoldersView;
        [SerializeField] private SavedSearchesView _savedSearchesView;
        [SerializeField] private CollectionsView _collectionsView;

        [Category("Main Area")] 
        [SerializeField] private SearchView _searchView;
        [SerializeField] private ItemsView _itemsView;

        [Category("Dialogs")] 
        [SerializeField] private AddSavedSearchDialog _addSavedSearchDialog;
        [SerializeField] private AddImportFolderDialog _addImportFolderDialog;
        [SerializeField] private ApplicationSettingsDialog _applicationSettingsDialog;

        [Category("Misc")] 
        [SerializeField] private PreviewCam _previewBuilder;
        [SerializeField] private ApplicationView _applicationView;

        private void Awake()
        {
            Texture.allowThreadedTextureCreation = true;
        }

        [SuppressMessage("ReSharper", "RedundantTypeArgumentsOfMethod")]
        private async void Start()
        {
            var aggregator = new MessageAggregator();

            IMessageRelay relay = aggregator;
            IConfigStore configStore = new AppDataConfigStore();
            IPreviewImageStore previewStore = new AppDataPreviewImageStore();
            
            var library = new Library(configStore, _previewBuilder, previewStore);
            var factory = new ImportFolderFactory(library);

            // Main View
            var applicationModel = new ApplicationModel(relay);
            var searchViewModel = new SearchModel(library, relay);
            var itemsViewModel = new ItemsModel(library, previewStore);

            // Main Menu
            var importFoldersViewModel = new ImportFoldersModel(configStore, factory, relay);
            var savedSearchesViewModel = new SavedSearchesModel(configStore, relay);
            var collectionsViewModel = new CollectionsModel(configStore, relay);

            // Dialogs
            var addSavedSearchViewModel = new AddSavedSearchModel(relay);
            var addImportFolderViewModel = new AddImportFolderModel(relay);
            var applicationSettingsModel = new ApplicationSettingsModel(configStore);
            
            BindViewModels();
            BindSettings();

            // Also restores app settings for import etc.
            await applicationSettingsModel.InitializeAsync();
            
            await library.InitializeAsync();
            await InitializeViewModels();

            aggregator.Subscribe(
                // Main View
                searchViewModel,
                itemsViewModel,

                // Main Menu
                importFoldersViewModel,
                savedSearchesViewModel,
                collectionsViewModel,

                // Dialogs
                addSavedSearchViewModel,
                addImportFolderViewModel,
                applicationSettingsModel);

            void BindSettings()
            {
                var rt = applicationSettingsModel.RuntimeSettings;

                rt.ImportParallelism.ValueChanged += factor => library.Parallelism = factor;
                rt.LogLevel.ValueChanged += logLevel => UnityLogger.LogLevel = logLevel;
                
                rt.UiScalePercent.ValueChanged += factor =>
                {
                    foreach (var canvas in FindObjectsOfType<Canvas>())
                    {
                        canvas.scaleFactor = applicationSettingsModel.UiScalePercent / 125f;
                    }
                };
            }

            void BindViewModels()
            {
                // Main View
                _applicationView.BindTo(applicationModel);
                _searchView.BindTo(searchViewModel);
                _itemsView.BindTo(itemsViewModel);

                // Main Menu
                _importFoldersView.BindTo(importFoldersViewModel);
                _savedSearchesView.BindTo(savedSearchesViewModel);
                _collectionsView.BindTo(collectionsViewModel);

                // Dialogs
                _addImportFolderDialog.BindTo(addImportFolderViewModel);
                _addSavedSearchDialog.BindTo(addSavedSearchViewModel);
                _applicationSettingsDialog.BindTo(applicationSettingsModel);
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