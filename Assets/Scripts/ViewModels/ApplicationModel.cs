using System;
using System.Windows.Input;
using JetBrains.Annotations;
using StlVault.Messages;
using StlVault.Util;
using StlVault.Util.Commands;
using StlVault.Util.Messaging;

namespace StlVault.ViewModels
{
    internal class ApplicationModel : ModelBase
    {
        [NotNull] private readonly IMessageRelay _relay;
        
        public ICommand ShowAppSettingsCommand { get; }

        public ApplicationModel([NotNull] IMessageRelay relay)
        {
            _relay = relay ?? throw new ArgumentNullException(nameof(relay));
            ShowAppSettingsCommand = new DelegateCommand(() => _relay.Send<RequestShowAppSettingsDialogMessage>(this));
        }
    }
}