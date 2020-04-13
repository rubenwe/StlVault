using System.Windows.Input;
using StlVault.Util;

namespace StlVault.ViewModels
{
    internal interface IDialogModel 
    {
        BindableProperty<bool> Shown { get; }
        ICommand AcceptCommand { get; }
        ICommand CancelCommand { get; }
    }
}