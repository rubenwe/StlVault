using System;
using JetBrains.Annotations;
using StlVault.Messages;
using StlVault.Util;
using StlVault.Util.Messaging;

namespace StlVault.ViewModels
{
    internal class AddCollectionModel : DialogModelBase<RequestShowDialogMessage.AddCollection>
    {
        public BindableProperty<string> Name { get; } = new BindableProperty<string>();
        
        private readonly IMessageRelay _relay;

        public AddCollectionModel([NotNull] IMessageRelay relay)
        {
            _relay = relay ?? throw new ArgumentNullException(nameof(relay));
            Name.ValueChanged += name => CanAcceptChanged();
        }

        protected override bool CanAccept() => !string.IsNullOrWhiteSpace(Name.Value);

        protected override void OnAccept()
        {
            _relay.Send(this, new AddCollectionMessage{Name = Name});
        }

        protected override void Reset(bool closing)
        {
            Name.Value = string.Empty;
        }
    }
}