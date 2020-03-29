using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using JetBrains.Annotations;
using StlVault.AppModel.Messages;
using StlVault.Config;
using StlVault.Messages;
using StlVault.Services;
using StlVault.Util;
using StlVault.Util.Collections;
using StlVault.Util.Commands;
using StlVault.Util.Messaging;
using UnityEngine;

namespace StlVault.ViewModels
{
    internal class ImportFoldersModel : ModelBase, IMessageReceiver<AddImportFolderMessage>
    {
        [NotNull] private readonly IConfigStore _store;
        [NotNull] private readonly IMessageRelay _relay;
        [NotNull] private readonly IImportFolderFactory _importFolderFactory;

        public ObservableList<ImportFolderModel> Folders { get; } = new ObservableList<ImportFolderModel>();
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
                Tags = message.Tags.OrderBy(tag => tag).Distinct().ToList(),
                Rotation = message.RotateOnImport ? message.Rotation : (Vector3?) null,
                Scale = message.ScaleOnImport ? message.Scale : (Vector3?) null,
                AutoTagMode = AutoTagMode.ExplodeSubDirPath
            };

            var folder = _importFolderFactory.Create(newConfig);

            var folders = SavedFolders
                .Append(folder)
                .OrderBy(f => f.Config.FullPath)
                .ToList();

            await SaveAndRefreshAsync(folders);
            await folder.InitializeAsync();
        }

        public async Task InitializeAsync()
        {
            var folderConfigs = await _store.LoadAsyncOrDefault<ImportFoldersConfigFile>();
            var folders = new List<IImportFolder>();
            var refreshTasks = new List<Task>();
            foreach (var folderConfig in folderConfigs)
            {
                folderConfig.AutoTagMode = AutoTagMode.ExplodeSubDirPath;

                var importFolder = _importFolderFactory.Create(folderConfig);
                folders.Add(importFolder);
                refreshTasks.Add(importFolder.InitializeAsync());
            }

            RefreshItems(folders);

            await Task.WhenAll(refreshTasks);
        }

        private List<IImportFolder> SavedFolders => Folders
            .Select(model => model.ImportFolder)
            .OrderBy(folder => folder.Config.FullPath)
            .ToList();

        private void RefreshItems(List<IImportFolder> importFolders)
        {
            var folders = importFolders
                .OrderBy(folder => folder.Config.FullPath)
                .Select(folder => new ImportFolderModel(folder, LoadItem, EditItem, DeleteItem));

            using (Folders.EnterMassUpdate())
            {
                Folders.Clear();
                Folders.AddRange(folders);
            }

            void LoadItem(ImportFolderModel item) =>
                _relay.Send(this, new SearchChangedMessage {SearchTags = new[] {"Folder: " + item.Path}});

            void EditItem(ImportFolderModel item)
            {
            }

            async void DeleteItem(ImportFolderModel item)
            {
                var currentFolders = SavedFolders;
                var importFolder = item.ImportFolder;

                currentFolders.Remove(importFolder);
                importFolder.Dispose();

                await SaveAndRefreshAsync(currentFolders);
            }
        }

        private async Task SaveAndRefreshAsync(List<IImportFolder> folders)
        {
            var fullConfig = new ImportFoldersConfigFile(folders.Select(f => f.Config));
            await _store.StoreAsync(fullConfig);

            RefreshItems(folders);
        }
    }
}