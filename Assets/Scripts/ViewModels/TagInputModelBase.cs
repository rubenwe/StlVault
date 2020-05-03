using System.Linq;
using System.Windows.Input;
using StlVault.Services;
using StlVault.Util;
using StlVault.Util.Collections;
using StlVault.Util.Commands;
using StlVault.Util.Tags;

namespace StlVault.ViewModels
{
    internal abstract class TagInputModelBase
    {
        public ObservableList<SuggestionModel> AutoCompletionSuggestions { get; } = new ObservableList<SuggestionModel>();
        public BindableProperty<string> CurrentInput { get; } = new BindableProperty<string>();
        public ObservableList<TagModel> Tags { get; } = new ObservableList<TagModel>();
        public ICommand PinCurrentInputCommand { get; }
     
        private readonly ILibrary _library;

        protected TagInputModelBase(ILibrary library)
        {
            _library = library;
            
            PinCurrentInputCommand = new DelegateCommand(CanPinCurrentInput, PinCurrentInput);
            CurrentInput.ValueChanged += UpdateAutoCompleteSuggestions;
        }
        
        protected abstract RecommendationMode RecommendationMode { get; }

        private void UpdateAutoCompleteSuggestions(string searchTerm)
        {
            using (AutoCompletionSuggestions.EnterMassUpdate())
            {
                AutoCompletionSuggestions.Clear();
                searchTerm = searchTerm?.Trim();
                if (string.IsNullOrEmpty(searchTerm)) return;

                var newSuggestions = _library
                    .GetRecommendations(Tags.Select(t => t.Text), searchTerm, RecommendationMode)
                    .Where(IsValidSuggestion)
                    .Select(result => new SuggestionModel(result.SearchTag, SuggestionChosen))
                    .Take(10);

                AutoCompletionSuggestions.AddRange(newSuggestions);
            }
        }

        protected virtual bool IsValidSuggestion(TagSearchResult result) => true;

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
            if (IsValidTag(tagText))
            {
                tagText = OnAddingTag(tagText);
                
                var sameTag = Tags.FirstOrDefault(t => t.Text == tagText);
                if (sameTag != null) Tags.Remove(sameTag);
                
                Tags.Add(new TagModel(tagText, RemoveTag));
                
                return true;
            }

            return false;
        }

        protected virtual bool IsValidTag(string tagText)
        {
            return !string.IsNullOrEmpty(tagText) && Tags.All(tag => tag.Text != tagText);
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