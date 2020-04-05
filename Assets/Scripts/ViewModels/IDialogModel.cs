using System.ComponentModel;
using System.Windows.Input;
using StlVault.Util;

namespace StlVault.ViewModels
{
    internal interface IDialogModel : INotifyPropertyChanged
    {
        BindableProperty<bool> Shown { get; }
        ICommand AcceptCommand { get; }
        ICommand CancelCommand { get; }
    }
}