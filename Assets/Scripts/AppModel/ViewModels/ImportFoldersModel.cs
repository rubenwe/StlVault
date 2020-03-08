using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using JetBrains.Annotations;
using StlVault.AppModel.Messages;
using StlVault.Config;
using StlVault.Util;
using StlVault.Util.Collections;
using StlVault.Util.Commands;
using StlVault.Util.Messaging;
using UnityEngine;

namespace StlVault.AppModel.ViewModels
{
    internal class ImportFoldersModel : ModelBase, IMessageReceiver<AddImportFolderMessage>
    {
        [NotNull] private readonly IConfigStore _store;
        [NotNull] private readonly IMessageRelay _relay;
        
        public ObservableList<ImportFolderModel> Folders { get; } = new ObservableList<ImportFolderModel>();
        public ICommand AddImportFolderCommand { get; }
        
        public ImportFoldersModel([NotNull] IConfigStore configStore, [NotNull] IMessageRelay relay)
        {
            _store = configStore ?? throw new ArgumentNullException(nameof(configStore));
            _relay = relay ?? throw new ArgumentNullException(nameof(relay));
            
            AddImportFolderCommand = new DelegateCommand(() => relay.Send<RequestShowAddImportFolderDialogMessage>(this));
        }

        public async void Receive(AddImportFolderMessage message)
        {
            var newConfig = new ImportFolderConfig
            {
                FullPath = Path.GetFullPath(message.FolderPath), 
                ScanSubDirectories = message.ScanSubDirectories,
                Tags = message.Tags.OrderBy(tag => tag).Distinct().ToList(),
                Rotation = message.RotateOnImport ? message.Rotation : (Vector3?) null,
                Scale = message.ScaleOnImport ? message.Scale : (Vector3?) null
            };
         
            var searches = SavedFolders;
            
            searches = searches.Append(newConfig).OrderBy(s => s.FullPath).ToList();
            await SaveAndRefreshAsync(searches);
        }

        public async Task Initialize()
        {
            var loadResult = await _store.TryLoadAsync<ImportFoldersConfig>();
            var config = loadResult ?? new ImportFoldersConfig();

            RefreshItems(config);
        }
        
        private List<ImportFolderConfig> SavedFolders => Folders
            .Select(s => s.Config)
            .OrderBy(s => s.FullPath)
            .ToList();
        
        private void RefreshItems(ImportFoldersConfig importFolders)
        {
            var folders = importFolders
                .OrderBy(s => s.FullPath)
                .Select(s => new ImportFolderModel(s, LoadItem, EditItem, DeleteItem));

            using (Folders.EnterMassUpdate())
            {
                Folders.Clear();
                Folders.AddRange(folders);
            }

            void LoadItem(ImportFolderModel item) =>
                _relay.Send(this, new SearchChangedMessage {SearchTags = new[]{"Folder: " + item.Path}});

            void EditItem(ImportFolderModel item)
            {
            }

            async void DeleteItem(ImportFolderModel item)
            {
                var current = SavedFolders;
                current.Remove(item.Config);
                await SaveAndRefreshAsync(current);
            }
        }
        
        private async Task SaveAndRefreshAsync(List<ImportFolderConfig> folders)
        {
            var fullConfig = new ImportFoldersConfig ( folders);

            await _store.StoreAsync(fullConfig);

            RefreshItems(fullConfig);
        }
    }
}