using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace StlVault.Util.Commands
{
    public static class CommandExtensions
    {
        public static bool CanExecute(this ICommand command)
        {
            return command.CanExecute(null);
        }

        public static void Execute(this ICommand command)
        {
            command.Execute(null);
        }

        public static async Task ExecuteAsync(this IAsyncCommand command)
        {
            await command.ExecuteAsync(null);
        }

        public static void OnCanExecuteChanged(this ICommand command)
        {
            if (command is ICanExecuteChange ce) ce.OnCanExecuteChanged();
            else throw new NotSupportedException("Can't update ExecuteChanged on non ICanExecuteChange commands!");
        }
    }
}