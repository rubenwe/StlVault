using System.Linq;
using StlVault.Messages;
using StlVault.Services;
using StlVault.Util.Messaging;

namespace StlVault.ViewModels
{
    internal class SearchModel : TagInputModelBase, IMessageReceiver<SearchChangedMessage>
    {
        private readonly IMessageRelay _relay;
        
        public SearchModel(ITagIndex tagIndex, IMessageRelay relay) : base(tagIndex)
        {
            _relay = relay;
        }

        protected override void OnTagAdded(string tag) => OnTagsChanged();
        protected override void OnTagRemoved(string tag) => OnTagsChanged();

        private void OnTagsChanged()
        {
            _relay.Send(this, new SearchChangedMessage {SearchTags = Tags.Select(tag => tag.Text).ToList()});
        }

        protected override string OnAddingTag(string tagText)
        {
            var exactMatch = AutoCompletionSuggestions.FirstOrDefault(suggestion => suggestion.Text == tagText);
            if (exactMatch == null)
            {
                var alternativeMatch = AutoCompletionSuggestions
                    .FirstOrDefault(suggestion => suggestion.Text.StartsWith(tagText))?.Text;
                
                tagText = alternativeMatch ?? tagText;
            }

            return tagText;
        }

        public void Receive(SearchChangedMessage message)
        {
            CurrentInput.Value = string.Empty;

            using (Tags.EnterMassUpdate())
            {
                Tags.Clear();
                Tags.AddRange(message.SearchTags.Select(text => new TagModel(text, RemoveTag)));
            }

            OnTagsChanged();
        }
    }
}