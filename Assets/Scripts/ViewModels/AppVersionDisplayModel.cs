using System;
using System.Windows.Input;
using JetBrains.Annotations;
using StlVault.Util;
using StlVault.Util.Commands;
using StlVault.Util.Messaging;
using UnityEngine;
using static StlVault.Messages.RequestShowDialogMessage;

namespace StlVault.ViewModels
{
    internal class AppVersionDisplayModel : IMessageReceiver<UpdateAvailable>
    {
        private readonly IMessageRelay _relay;
        private readonly BindableProperty<UpdateAvailable> _message = new BindableProperty<UpdateAvailable>();

        public BindableProperty<string> CurrentVersion { get; } = new BindableProperty<string>();
        public ICommand OpenUpdateDialogCommand { get; }
        
        public AppVersionDisplayModel([NotNull] IMessageRelay relay)
        {
            _relay = relay ?? throw new ArgumentNullException(nameof(relay));
            
            CurrentVersion.Value = "v" + Application.version;
            OpenUpdateDialogCommand = new DelegateCommand(UpdateAvailable, ForceShowUpdateDialog)
                .UpdateOn(_message);
        }

        private void ForceShowUpdateDialog()
        {
            var message = _message.Value;
            message.UserRequested = true;
            _relay.Send(this, message);
        }

        private bool UpdateAvailable() => _message.Value != null;

        public void Receive(UpdateAvailable message) => _message.Value = message;
    }
}