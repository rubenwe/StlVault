using System.Windows.Input;

namespace StlVault.Util.Commands
{
    public interface ICommand<in T> : ICommand
    {
        bool CanExecute(T param);
        void Execute(T param);
    }
}