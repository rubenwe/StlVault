using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Input;

namespace StlVault.Util.Commands
{
    public interface IAsyncCommand : ICommand, INotifyPropertyChanged, ICanExecuteChange
    {
        IAsyncExecution Execution { get; }
        Task ExecuteAsync(object parameter);
    }
}