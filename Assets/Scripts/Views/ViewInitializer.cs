using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using StlVault.AppModel;
using StlVault.AppModel.ViewModels;
using StlVault.Stl;
using StlVault.Util.Messaging;
using UnityEngine;
using Debug = UnityEngine.Debug;

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
        
        [Category("Misc")]
        [SerializeField] private PreviewCam _previewCam;
        
        
        private CancellationTokenSource _import;
        
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
            
            // var folderManager = new ImportFolderManager(store);
            
            var library = new Library(store);
            
            // Main View
            var searchViewModel = new SearchModel(library, relay);
            var itemsViewModel = new ItemsModel(library);
            
            // Main Menu
            var importFoldersViewModel = new ImportFoldersModel(store, relay);
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

            RebuildPreviews(library.GetItemPreviewMetadata(new List<string>()));

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
                await library.InitializeAsync();
                
                await savedSearchesViewModel.Initialize();
                await importFoldersViewModel.Initialize();
                await collectionsViewModel.Initialize();
            }
        }

        private async void RebuildPreviews(IReadOnlyList<ItemPreviewMetadata> itemMetaData)
        {
            await Task.Delay(1);
            
            _import = new CancellationTokenSource();
            var token = _import.Token;

            var sw = Stopwatch.StartNew();
            foreach (var item in itemMetaData)
            {
                if (token.IsCancellationRequested) return;
                if (File.Exists(item.PreviewImagePath)) continue;
                
                await BuildPreview(item);
            }
            
            Debug.Log($"Finished import of {itemMetaData.Count} items in {sw.Elapsed.TotalSeconds}s.");
        }

        private void OnDestroy()
        {
            _import?.Cancel();
        }

        private async Task BuildPreview(ItemPreviewMetadata obj)
        {
            var sw = Stopwatch.StartNew();
            var sb = new StringBuilder();
            
            var (mesh, hash) = await StlImporter.ImportMeshAsync(obj.StlFilePath);

            sb.AppendLine($"Imported {obj.ItemName} - Took {sw.ElapsedMilliseconds}ms.");
            sb.AppendLine($"Vertices: {mesh.vertexCount}, Hash: {hash}");
            
            var snapshot = _previewCam.GetSnapshot(mesh, obj.Rotation, 80);
            File.WriteAllBytes(obj.PreviewImagePath, snapshot);
                
            Destroy(mesh);
            
            Debug.Log(sb.ToString());
        }
    }
}