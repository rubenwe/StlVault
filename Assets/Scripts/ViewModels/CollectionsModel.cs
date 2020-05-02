using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using JetBrains.Annotations;
using StlVault.Config;
using StlVault.Messages;
using StlVault.Services;
using StlVault.Util;
using StlVault.Util.Collections;
using StlVault.Util.Commands;
using StlVault.Util.Messaging;

namespace StlVault.ViewModels
{
    internal class CollectionsModel : 
        IMessageReceiver<AddCollectionMessage>, 
        IMessageReceiver<SelectionChangedMessage>
    {
        [NotNull] private readonly ObservableSet<(string hash, ItemPreviewModel model)> _selected = new ObservableSet<(string, ItemPreviewModel)>();
        [NotNull] private readonly IConfigStore _store;
        [NotNull] private readonly IMessageRelay _relay;
        [NotNull] private readonly ILibrary _library;
        [NotNull] private readonly CollectionCommands _commands;

        private CollectionModel _selection;
        private List<string> SelectedHashes => _selected.Select(p => p.hash).ToList();

        public ObservableList<CollectionModel> Collections { get; } = new ObservableList<CollectionModel>();
        public ICommand AddCollectionCommand { get; }

        public CollectionsModel([NotNull] IConfigStore store, [NotNull] ILibrary library, [NotNull] IMessageRelay relay)
        {
            _store = store ?? throw new ArgumentNullException(nameof(store));
            _relay = relay ?? throw new ArgumentNullException(nameof(relay));
            _library = library ?? throw new ArgumentNullException(nameof(library));

            AddCollectionCommand = new DelegateCommand(
                    () => _selected.Any(),
                    () => _relay.Send(this, new RequestShowDialogMessage.AddCollection()))
                .UpdateOn(_selected);
            
            _commands = new CollectionCommands
            {
                Add = new DelegateCommand<CollectionModel>(CanAdd, Add).UpdateOn(_selected),
                Delete = new DelegateCommand<CollectionModel>(CanDelete, Delete).UpdateOn(_selected),
                Select = new DelegateCommand<CollectionModel>(_ => true, Select)
            };
        }

        public async Task InitializeAsync()
        {
            var data = await _store.LoadAsyncOrDefault<CollectionsConfigFile>();
            
            var selectionStream = _library.GetItemPreviewMetadata(new[] {"collection: selected"});
            _selection = new CollectionModel(new CollectionConfig{Name = "Selected"}, selectionStream, _commands);
            Collections.Add(_selection);
            
            foreach (var config in data)
            {
                var filters = GetFilters(config);
                var stream = _library.GetItemPreviewMetadata(filters);
                var model = new CollectionModel(config, stream, _commands);
                
                Collections.Add(model);
            }
        }

        private bool CanAdd(CollectionModel model)
        {
            return _selected.Any() && model.Config.Name != "Selected";
        }
        
        private void Add(CollectionModel model)
        {
            if (!CanAdd(model)) return;
            _library.AddTag(SelectedHashes, GetFilter(model.Config));
        }

        private void Select(CollectionModel model)
        {
            var filters = GetFilters(model.Config);
            _relay.Send(this, new SearchChangedMessage {SearchTags = filters});
        }
        
        private IDisposable NotifyMassSelection()
        {
            _relay.Send<MassSelectionStartingMessage>(this);
            return Disposable.FromAction(() => _relay.Send<MassSelectionFinishedMessage>(this));
        }

        private bool CanDelete(CollectionModel model)
        {
            return model.Config.Name != "Selected" || _selected.Any();
        }

        private async void Delete(CollectionModel collection)
        {
            if (collection.Config.Name == "Selected")
            {
                DeselectAll();
                return;
            }
            
            var filter = GetFilter(collection.Config);
            var collectionItems = _library.GetAllItems()
                .Where(i => i.Tags.Contains(filter))
                .Select(i => i.FileHash)
                .ToList();
            
            _library.RemoveTag(collectionItems, filter);
            Collections.Remove(collection);

            await StoreConfigAsync();
        }

        private void DeselectAll()
        {
            using (NotifyMassSelection())
            {
                var models = _selected.Select(s => s.model).ToList();
                foreach (var preview in models)
                {
                    preview.Selected.Value = false;
                }
            }
        }

        private static string GetFilter(CollectionConfig config) => $"collection: {config.Name.ToLowerInvariant()}";
        private static string[] GetFilters(CollectionConfig config) => new [] {GetFilter(config)};

        public async void Receive(AddCollectionMessage message)
        {
            var existing = Collections.FirstOrDefault(c => c.Config.Name == message.Name);
            if (existing != null)
            {
                if (message.Name.ToLowerInvariant() == "selected") return;
                _library.AddTag(SelectedHashes, GetFilter(existing.Config));
                return;
            }
            
            var config = new CollectionConfig{Name = message.Name};
            _library.AddTag(SelectedHashes, GetFilter(config));

            var stream = _library.GetItemPreviewMetadata(GetFilters(config));
            var model = new CollectionModel(config, stream, _commands);

            using (Collections.EnterMassUpdate())
            {
                var current = Collections.Where(c => c != _selection).ToList();
                Collections.Clear();
                
                var sorted = current.Append(model).OrderBy(cm => cm.Label.Value);
                Collections.Add(_selection);
                Collections.AddRange(sorted);
            }

            await StoreConfigAsync();
        }

        private async Task StoreConfigAsync()
        {
            var file = new CollectionsConfigFile();
            var customCollections = Collections.Where(c => c != _selection).Select(c => c.Config);
            file.AddRange(customCollections);

            await _store.StoreAsync(file);
        }

        public void Receive(SelectionChangedMessage message)
        {
            var preview = message.Sender;
            if (preview.Selected) _selected.Add((preview.FileHash, preview));
            else _selected.Remove((preview.FileHash, preview));
        }
    }
}