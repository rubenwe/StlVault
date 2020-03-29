using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using StlVault.Config;
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
        [Category("Main Menu")] [SerializeField]
        private ImportFoldersView _importFoldersView;

        [SerializeField] private SavedSearchesView _savedSearchesView;
        [SerializeField] private CollectionsView _collectionsView;

        [Category("Main Area")] [SerializeField]
        private SearchView _searchView;

        [SerializeField] private ItemsView _itemsView;

        [Category("Dialogs")] [SerializeField] private AddSavedSearchDialog _addSavedSearchDialog;
        [SerializeField] private AddImportFolderDialog _addImportFolderDialog;

        [Category("Misc")] [SerializeField] private PreviewBuilder _previewBuilder;

        private void Awake()
        {
            Texture.allowThreadedTextureCreation = true;
        }

        [SuppressMessage("ReSharper", "RedundantTypeArgumentsOfMethod")]
        private async void Start()
        {
            var aggregator = new MessageAggregator();

            IMessageRelay relay = aggregator;
            IConfigStore store = new AppDataConfigStore();

            UpdateApplicationSettings(store);

            var library = new Library(_previewBuilder);
            var factory = new ImportFolderFactory(new AppDataKnownItemStore(), library);

            // Main View
            var searchViewModel = new SearchModel(library, relay);
            var itemsViewModel = new ItemsModel(library);

            // Main Menu
            var importFoldersViewModel = new ImportFoldersModel(store, factory, relay);
            var savedSearchesViewModel = new SavedSearchesModel(store, relay);
            var collectionsViewModel = new CollectionsModel(store, relay);

            // Dialogs
            var addSavedSearchViewModel = new AddSavedSearchModel(relay);
            var addImportFolderViewModel = new AddImportFolderModel(relay);

            BindViewModels();
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
                addImportFolderViewModel);

            void BindViewModels()
            {
                // Main View
                _searchView.BindTo(searchViewModel);
                _itemsView.BindTo(itemsViewModel);

                // Main Menu
                _importFoldersView.BindTo(importFoldersViewModel);
                _savedSearchesView.BindTo(savedSearchesViewModel);
                _collectionsView.BindTo(collectionsViewModel);

                // Dialogs
                _addImportFolderDialog.BindTo(addImportFolderViewModel);
                _addSavedSearchDialog.BindTo(addSavedSearchViewModel);
            }

            async Task InitializeViewModels()
            {
                await savedSearchesViewModel.InitializeAsync();
                await importFoldersViewModel.InitializeAsync();
                await collectionsViewModel.Initialize();
            }
        }

        private static void UpdateApplicationSettings(IConfigStore store)
        {
            var settings = store.LoadOrDefault<ApplicationSettings>();
            foreach (var canvas in FindObjectsOfType<Canvas>())
            {
                canvas.scaleFactor = settings.UiScalePercent / 125f;
            }

            UnityLogger.LogLevel = settings.LogLevel;
        }
    }
}