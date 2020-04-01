using System.ComponentModel;
using System.Windows.Input;

namespace StlVault.ViewModels
{
    internal interface IDialogModel : INotifyPropertyChanged
    {
        bool Shown { get; }
        ICommand AcceptCommand { get; }
        ICommand CancelCommand { get; }
    }
}