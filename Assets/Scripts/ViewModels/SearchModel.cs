using System.Linq;
using System.Windows.Input;
using StlVault.AppModel.Messages;
using StlVault.Services;
using StlVault.Util;
using StlVault.Util.Collections;
using StlVault.Util.Commands;
using StlVault.Util.Messaging;

namespace StlVault.ViewModels
{
    internal class SearchModel : ModelBase, IMessageReceiver<SearchChangedMessage>
    {
        private readonly ObservableList<SuggestionModel> _autoCompletionSuggestions =
            new ObservableList<SuggestionModel>();

        private readonly ObservableList<TagModel> _searchedTags = new ObservableList<TagModel>();
        private readonly IMessageRelay _relay;
        private readonly ITagIndex _tagIndex;

        private string _currentSearchInput;

        public IReadOnlyObservableList<SuggestionModel> AutoCompletionSuggestions => _autoCompletionSuggestions;
        public IReadOnlyObservableList<TagModel> SearchedTags => _searchedTags;
        public ICommand PinCurrentInputCommand { get; }

        public string CurrentSearchInput
        {
            get => _currentSearchInput;
            set
            {
                if (SetValueAndNotify(ref _currentSearchInput, value))
                {
                    UpdateAutoCompleteSuggestions(value);
                }
            }
        }

        private void UpdateAutoCompleteSuggestions(string searchTerm)
        {
            _autoCompletionSuggestions.Clear();
            searchTerm = searchTerm?.Trim();
            if (string.IsNullOrEmpty(searchTerm)) return;

            var newSuggestions = _tagIndex
                .GetRecommendations(_searchedTags.Select(t => t.Text), searchTerm)
                .Select(result => new SuggestionModel(result.SearchTag, SuggestionChosen))
                .Take(10);

            _autoCompletionSuggestions.AddRange(newSuggestions);
        }

        private void SuggestionChosen(SuggestionModel suggestion)
        {
            AddTag(suggestion.Text);
            CurrentSearchInput = string.Empty;
            TriggerSearch();
        }

        private void AddTag(string tagText)
        {
            tagText = tagText?.Trim().ToLowerInvariant();
            if (string.IsNullOrEmpty(tagText)) return;

            if (_searchedTags.All(tag => tag.Text != tagText))
            {
                var exactMatch = AutoCompletionSuggestions.FirstOrDefault(suggestion => suggestion.Text == tagText);
                if (exactMatch == null)
                {
                    var alternativeMatch = AutoCompletionSuggestions
                        .FirstOrDefault(suggestion => suggestion.Text.StartsWith(tagText))?.Text;
                    tagText = alternativeMatch ?? tagText;
                }

                _searchedTags.Add(new TagModel(tagText, RemoveTag));
            }
        }

        public SearchModel(ITagIndex tagIndex, IMessageRelay relay)
        {
            _relay = relay;
            _tagIndex = tagIndex;

            PinCurrentInputCommand = new DelegateCommand(
                () => !string.IsNullOrWhiteSpace(CurrentSearchInput),
                PinCurrentInput);
        }

        private void PinCurrentInput()
        {
            AddTag(CurrentSearchInput);
            CurrentSearchInput = string.Empty;
            TriggerSearch();
        }

        private void RemoveTag(TagModel removedTag)
        {
            _searchedTags.Remove(removedTag);
            TriggerSearch();
        }

        private void TriggerSearch()
        {
            _relay.Send(this, new SearchChangedMessage {SearchTags = _searchedTags.Select(tag => tag.Text).ToList()});
        }

        public void Receive(SearchChangedMessage message)
        {
            CurrentSearchInput = string.Empty;

            using (_searchedTags.EnterMassUpdate())
            {
                _searchedTags.Clear();
                _searchedTags.AddRange(message.SearchTags.Select(text => new TagModel(text, RemoveTag)));
            }

            TriggerSearch();
        }
    }
}