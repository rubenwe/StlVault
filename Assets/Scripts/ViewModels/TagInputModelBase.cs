using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using StlVault.Services;
using StlVault.Util;
using StlVault.Util.Collections;
using StlVault.Util.Commands;

namespace StlVault.ViewModels
{
    internal abstract class TagInputModelBase
    {
        public ObservableList<SuggestionModel> AutoCompletionSuggestions { get; } = new ObservableList<SuggestionModel>();
        public BindableProperty<string> CurrentInput { get; } = new BindableProperty<string>();
        public ObservableList<TagModel> Tags { get; } = new ObservableList<TagModel>();
        public ICommand PinCurrentInputCommand { get; }
     
        private readonly ITagIndex _tagIndex;

        protected TagInputModelBase(ITagIndex tagIndex)
        {
            _tagIndex = tagIndex;
            
            PinCurrentInputCommand = new DelegateCommand(CanPinCurrentInput, PinCurrentInput);
            CurrentInput.ValueChanged += UpdateAutoCompleteSuggestions;
        }

        private void UpdateAutoCompleteSuggestions(string searchTerm)
        {
            using (AutoCompletionSuggestions.EnterMassUpdate())
            {
                AutoCompletionSuggestions.Clear();
                searchTerm = searchTerm?.Trim();
                if (string.IsNullOrEmpty(searchTerm)) return;

                var newSuggestions = _tagIndex
                    .GetRecommendations(Tags.Select(t => t.Text), searchTerm)
                    .Select(result => new SuggestionModel(result.SearchTag, SuggestionChosen))
                    .Take(10);

                AutoCompletionSuggestions.AddRange(newSuggestions);
            }
        }

        private void SuggestionChosen(SuggestionModel suggestion)
        {
            if (AddTag(suggestion.Text))
            {
                OnTagAdded(suggestion.Text);
            }
            
            CurrentInput.Value = string.Empty;
        }

        private bool AddTag(string tagText)
        {
            tagText = tagText?.Trim().ToLowerInvariant();
            if (!string.IsNullOrEmpty(tagText) && Tags.All(tag => tag.Text != tagText))
            {
                tagText = OnAddingTag(tagText);
                Tags.Add(new TagModel(tagText, RemoveTag));
                
                return true;
            }

            return false;
        }

        protected virtual string OnAddingTag(string tagText) => tagText;

        protected virtual bool CanPinCurrentInput()
        {
            return !string.IsNullOrWhiteSpace(CurrentInput);
        }

        private void PinCurrentInput()
        {
            if (AddTag(CurrentInput))
            {
                OnTagAdded(CurrentInput);
            }
            
            CurrentInput.Value = string.Empty;
        }

        protected void RemoveTag(TagModel removedTag)
        {
            Tags.Remove(removedTag);
            OnTagRemoved(removedTag.Text);
        }

        protected virtual void OnTagAdded(string tag) { }
        protected virtual void OnTagRemoved(string tag) { }
    }
}