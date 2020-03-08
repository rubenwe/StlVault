using System.ComponentModel;
using System.Windows.Input;

namespace StlVault.AppModel.ViewModels
{
    internal interface IDialogViewModel : INotifyPropertyChanged
    {
        bool Shown { get; }
        ICommand AcceptCommand { get; }
        ICommand CancelCommand { get; }
    }
}