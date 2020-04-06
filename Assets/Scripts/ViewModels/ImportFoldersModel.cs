using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using JetBrains.Annotations;
using StlVault.Config;
using StlVault.Messages;
using StlVault.Services;
using StlVault.Util.Collections;
using StlVault.Util.Commands;
using StlVault.Util.Messaging;
using StlVault.Views;
using UnityEngine;

namespace StlVault.ViewModels
{
    internal sealed class ImportFoldersModel : IMessageReceiver<AddImportFolderMessage>
    {
        [NotNull] private readonly IConfigStore _store;
        [NotNull] private readonly IMessageRelay _relay;
        [NotNull] private readonly IImportFolderFactory _importFolderFactory;
        
        public ObservableList<FileSourceModel> Folders { get; } = new ObservableList<FileSourceModel>();
        public ICommand AddImportFolderCommand { get; }

        public ImportFoldersModel(
            [NotNull] IConfigStore configStore,
            [NotNull] IImportFolderFactory importFolderFactory,
            [NotNull] IMessageRelay relay)
        {
            _store = configStore ?? throw new ArgumentNullException(nameof(configStore));
            _relay = relay ?? throw new ArgumentNullException(nameof(relay));
            _importFolderFactory = importFolderFactory ?? throw new ArgumentNullException(nameof(importFolderFactory));

            AddImportFolderCommand =
                new DelegateCommand(() => relay.Send<RequestShowAddImportFolderDialogMessage>(this));
        }

        public async void Receive(AddImportFolderMessage message)
        {
            var newConfig = new ImportFolderConfig
            {
                FullPath = Path.GetFullPath(message.FolderPath),
                ScanSubDirectories = message.ScanSubDirectories,
                AdditionalTags = message.Tags.OrderBy(tag => tag).Distinct().ToList(),
                Rotation = message.RotateOnImport ? message.Rotation : (Vector3?) null,
                Scale = message.ScaleOnImport ? message.Scale : (Vector3?) null,
                AutoTagMode = AutoTagMode.ExplodeResourcePath
            };

            var folder = _importFolderFactory.Create(newConfig);

            var folders = SavedFolders
                .Append(folder)
                .OrderBy(f => f.DisplayName)
                .ToList();

            await SaveAndRefreshAsync(folders);
            _ = folder.InitializeAsync();
            
            // Switch to added folder
            _relay.Send(this, new SearchChangedMessage {SearchTags = new[] {"Folder: " + newConfig.FullPath}});
        }

        public async Task InitializeAsync()
        {
            var folderConfigs = await _store.LoadAsyncOrDefault<ImportFoldersConfigFile>();
            var folders = new List<IImportFolder>();
            
            foreach (var folderConfig in folderConfigs)
            {
                folderConfig.AutoTagMode = AutoTagMode.ExplodeResourcePath;

                var importFolder = _importFolderFactory.Create(folderConfig);
                folders.Add(importFolder);
                _ = importFolder.InitializeAsync();
            }

            RefreshItems(folders);
        }

        private List<IImportFolder> SavedFolders => Folders
            .Select(model => model.FileSource)
            .OfType<IImportFolder>()
            .OrderBy(folder => folder.DisplayName)
            .ToList();

        private void RefreshItems(List<IImportFolder> importFolders)
        {
            var folders = importFolders
                .OrderBy(folder => folder.DisplayName)
                .Select(folder => new FileSourceModel(folder, LoadItem, EditItem, DeleteItem));

            using (Folders.EnterMassUpdate())
            {
                Folders.Clear();
                Folders.AddRange(folders);
            }

            void LoadItem(FileSourceModel model) =>
                _relay.Send(this, new SearchChangedMessage {SearchTags = new[] {"Folder: " + model.Path}});

            void EditItem(FileSourceModel mode)
            {
            }

            async void DeleteItem(FileSourceModel model)
            {
                var currentFolders = SavedFolders;
                var importFolder = (ImportFolder) model.FileSource;

                currentFolders.Remove(importFolder);
                
                await importFolder.OnDeletedAsync();
                await SaveAndRefreshAsync(currentFolders);
            }
        }

        private async Task SaveAndRefreshAsync(List<IImportFolder> folders)
        {
            var fullConfig = new ImportFoldersConfigFile(folders.Select(f => f.Config).OfType<ImportFolderConfig>());
            await _store.StoreAsync(fullConfig);

            RefreshItems(folders);
        }

        ~ImportFoldersModel()
        {
            // Kill file watcher
            foreach (var folder in Folders.Select(model => model.FileSource).OfType<IImportFolder>())
            {
                folder.Dispose();
            }
        }
    }
}