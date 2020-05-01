using System;
using System.Collections.Generic;
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
using UnityEngine;

namespace StlVault.ViewModels
{
    internal sealed class ImportFoldersModel : IMessageReceiver<AddImportFolderMessage>, IDisposable
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
                new DelegateCommand(() => relay.Send<RequestShowDialogMessage.AddImportFolder>(this));
        }

        public async void Receive(AddImportFolderMessage message)
        {
            var newConfig = new ImportFolderConfig
            {
                Alias = message.Alias,
                FullPath = Path.GetFullPath(message.FolderPath),
                ScanSubDirectories = message.ScanSubDirectories,
                AdditionalTags = message.Tags.OrderBy(tag => tag).Distinct().ToList(),
                Rotation = message.RotateOnImport ? message.Rotation : (Vector3?) null,
                Scale = message.ScaleOnImport ? message.Scale : (Vector3?) null,
                AutoTagMode = AutoTagMode.ExplodeResourcePath
            };

            if (SavedFolders.Any(dir => dir.Id == newConfig.FullPath))
            {
                _relay.Send(this, new ProgressMessage{Text = $"The folder `{newConfig.FullPath}` can not be added a second time!"});
                return;
            }
            
            var folder = _importFolderFactory.Create(newConfig);
            
            var folders = SavedFolders
                .Append(folder)
                .OrderBy(f => f.DisplayName)
                .ToList();

            await SaveAndRefreshAsync(folders);
            _ = folder.InitializeAsync();
            
            // Switch to added folder
            _relay.Send(this, new SearchChangedMessage {SearchTags = new[] {"folder: " + newConfig.FullPath.ToLowerInvariant()}});
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

            void LoadItem(FileSourceModel model)
            {
                var msg = new SearchChangedMessage {SearchTags = new[] {"folder: " + model.FileSource.Id.ToLowerInvariant()}};
                _relay.Send(this, msg);
            }

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

        public void Dispose()
        {
            foreach (var model in Folders)
            {
                // Kill file watcher
               model.FileSource.Dispose();
            }
        }

        ~ImportFoldersModel() => Dispose();
    }
}