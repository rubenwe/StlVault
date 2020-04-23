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
            Reset(true);
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
            Reset(true);
        }

        public void Receive(TShowMessage message)
        {
            if (ShouldShow(message))
            {
                Show(message);
            }
        }

        protected virtual bool ShouldShow(TShowMessage message) => true;
        
        protected virtual void OnShown(TShowMessage message)
        {
        }

        private void Show(TShowMessage message)
        {
            Reset(false);
            OnShown(message);
            Shown.Value = true;
        }

        protected abstract void Reset(bool closing);
    }
}