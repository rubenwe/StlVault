using System.ComponentModel;
using System.Windows.Input;
using StlVault.Util;
using StlVault.Util.Commands;
using StlVault.Util.Messaging;

namespace StlVault.ViewModels
{
    internal abstract class DialogModelBase<TShowMessage> : IDialogModel, IMessageReceiver<TShowMessage>
    {
        public BindableProperty<bool> Shown { get; } = new BindableProperty<bool>();

        public ICommand AcceptCommand { get; }
        public ICommand CancelCommand { get; }

        protected DialogModelBase()
        {
            AcceptCommand = new DelegateCommand(CanAccept, Accept);
            CancelCommand = new DelegateCommand(CanCancel, Cancel);
        }

        protected void CanAcceptChanged() => AcceptCommand.OnCanExecuteChanged();
        protected virtual bool CanAccept() => true;
        protected abstract void OnAccept();

        private void Accept()
        {
            OnAccept();
            Shown.Value = false;
            Reset();
        }

        protected void CanCancelChanged() => CancelCommand.OnCanExecuteChanged();
        protected virtual bool CanCancel() => true;

        protected virtual void OnCancel()
        {
        }

        private void Cancel()
        {
            OnCancel();
            Shown.Value = false;
            Reset();
        }

        public void Receive(TShowMessage message) => Show(message);

        protected virtual void OnShown(TShowMessage message)
        {
        }

        private void Show(TShowMessage message)
        {
            Reset();
            OnShown(message);
            Shown.Value = true;
        }

        protected abstract void Reset();
    }
}