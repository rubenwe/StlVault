using System;
using System.Windows.Input;
using JetBrains.Annotations;
using StlVault.Messages;
using StlVault.Util.Commands;
using StlVault.Util.Messaging;
using UnityEngine;

namespace StlVault.ViewModels
{
    internal class ApplicationModel
    {
        public ICommand ShowAppSettingsCommand { get; }
        public ICommand ShowFeedbackCommand { get; }
        public ICommand ShowHelpCommand { get; }

        public ApplicationModel([NotNull] IMessageRelay relay)
        {
           if(relay is null) throw new ArgumentNullException(nameof(relay));
            ShowAppSettingsCommand = new DelegateCommand(() => relay.Send<RequestShowDialogMessage.AppSettings>(this));
            ShowFeedbackCommand = new DelegateCommand(() => relay.Send<RequestShowDialogMessage.UserFeedback>(this));
            ShowHelpCommand = new DelegateCommand(OpenWiki);
        }

        private void OpenWiki()
        {
            Application.OpenURL(@"https://github.com/rubenwe/StlVault/wiki");
        }
    }
}