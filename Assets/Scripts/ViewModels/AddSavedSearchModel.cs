using System.Collections.Generic;
using StlVault.Messages;
using StlVault.Util;
using StlVault.Util.Messaging;

namespace StlVault.ViewModels
{
    internal class AddSavedSearchModel : DialogModelBase<RequestShowDialogMessage.SaveSearch>
    {
        private readonly IMessageRelay _relay;
        private IReadOnlyList<string> _searchTags;

        public BindableProperty<string> Alias { get; } = new BindableProperty<string>();
        public AddSavedSearchModel(IMessageRelay relay)
        {
            _relay = relay;
            Alias.ValueChanged += alias => CanAcceptChanged();
        }

        protected override bool CanAccept() => !string.IsNullOrWhiteSpace(Alias);

        protected override void OnAccept()
        {
            _relay.Send(this, new SaveSearchMessage
            {
                Alias = Alias,
                SearchTags = _searchTags
            });
        }

        protected override void Reset(bool closing)
        {
            Alias.Value = string.Empty;
            _searchTags = null;
        }

        protected override void OnShown(RequestShowDialogMessage.SaveSearch message)
        {
            _searchTags = message.SearchTags;
        }
    }
}