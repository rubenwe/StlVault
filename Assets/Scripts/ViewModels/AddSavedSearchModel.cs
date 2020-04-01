using System.Collections.Generic;
using StlVault.Messages;
using StlVault.Util.Messaging;

namespace StlVault.ViewModels
{
    internal class AddSavedSearchModel : DialogModelBase<RequestShowSaveSearchDialogMessage>
    {
        private readonly IMessageRelay _relay;
        private string _alias;
        private IReadOnlyList<string> _searchTags;

        public string Alias
        {
            get => _alias;
            set
            {
                if (SetValueAndNotify(ref _alias, value)) CanAcceptChanged();
            }
        }

        public AddSavedSearchModel(IMessageRelay relay) => _relay = relay;

        protected override bool CanAccept() => !string.IsNullOrWhiteSpace(Alias);

        protected override void OnAccept()
        {
            _relay.Send(this, new SaveSearchMessage
            {
                Alias = Alias,
                SearchTags = _searchTags
            });
        }

        protected override void Reset()
        {
            Alias = string.Empty;
            _searchTags = null;
        }

        protected override void OnShown(RequestShowSaveSearchDialogMessage message)
        {
            _searchTags = message.SearchTags;
        }
    }
}