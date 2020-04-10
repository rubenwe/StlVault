using System;
using System.Collections.Generic;
using System.ComponentModel;
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

namespace StlVault.ViewModels
{
    internal class SavedSearchesModel : IMessageReceiver<SearchChangedMessage>,
        IMessageReceiver<SaveSearchMessage>
    {
        private IReadOnlyList<string> _currentSearchTags;

        private readonly IConfigStore _store;
        private readonly IMessageRelay _relay;

        public ICommand SaveCurrentSearchCommand { get; }
        public ObservableList<SavedSearchModel> SavedSearches { get; } = new ObservableList<SavedSearchModel>();

        public SavedSearchesModel([NotNull] IConfigStore store, [NotNull] IMessageRelay relay)
        {
            _store = store ?? throw new ArgumentNullException(nameof(store));
            _relay = relay ?? throw new ArgumentNullException(nameof(relay));
            SaveCurrentSearchCommand = new DelegateCommand(CanSaveCurrentSearch, SaveCurrentSearch);
        }

        public void Receive(SearchChangedMessage message)
        {
            _currentSearchTags = message.SearchTags;
            SaveCurrentSearchCommand.OnCanExecuteChanged();
        }

        private bool CanSaveCurrentSearch() => _currentSearchTags?.Any() == true;

        private void SaveCurrentSearch()
        {
            if (!CanSaveCurrentSearch()) return;
            _relay.Send(this, new RequestShowDialogMessage.SaveSearch {SearchTags = _currentSearchTags});
        }

        public async void Receive(SaveSearchMessage message)
        {
            var newConfig = new SavedSearchConfig
            {
                Alias = message.Alias,
                Tags = message.SearchTags.OrderBy(tag => tag).ToList()
            };

            var searches = SavedSearchConfigs;

            RemoveDuplicates(searches, newConfig);

            searches = searches.Append(newConfig).OrderBy(s => s.Alias).ToList();
            await SaveAndRefreshAsync(searches);
        }

        private List<SavedSearchConfig> SavedSearchConfigs => SavedSearches
            .Select(s => s.Config)
            .OrderBy(s => s.Alias)
            .ToList();

        private async Task SaveAndRefreshAsync(List<SavedSearchConfig> searches)
        {
            var fullConfig = new SavedSearchesConfigFile(searches);

            await _store.StoreAsync(fullConfig);

            RefreshItems(fullConfig);
        }

        private static void RemoveDuplicates(List<SavedSearchConfig> searches, SavedSearchConfig newConfig)
        {
            foreach (var search in searches.ToList())
            {
                bool SameTags() => search.Tags.OrderBy(tag => tag)
                    .SequenceEqual(newConfig.Tags);

                if (search.Alias == newConfig.Alias || SameTags())
                {
                    searches.Remove(search);
                }
            }
        }

        public async Task InitializeAsync()
        {
            var config = await _store.LoadAsyncOrDefault<SavedSearchesConfigFile>();
            RefreshItems(config);
        }

        private void RefreshItems(SavedSearchesConfigFile savedSearches)
        {
            var searches = savedSearches
                .OrderBy(s => s.Alias)
                .Select(s => new SavedSearchModel(s, LoadItem, DeleteItem));

            using (SavedSearches.EnterMassUpdate())
            {
                SavedSearches.Clear();
                SavedSearches.AddRange(searches);
            }

            void LoadItem(SavedSearchModel item) =>
                _relay.Send(this, new SearchChangedMessage {SearchTags = item.Tags});

            async void DeleteItem(SavedSearchModel item)
            {
                var configs = SavedSearchConfigs;
                configs.Remove(item.Config);
                await SaveAndRefreshAsync(configs);
            }
        }
    }
}