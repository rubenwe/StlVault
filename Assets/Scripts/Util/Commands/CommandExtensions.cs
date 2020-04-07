using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Input;

namespace StlVault.Util.Commands
{
    internal static class CommandExtensions
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

        public static TCommand UpdateOn<TCommand, TProp>(this TCommand command, IBindableProperty<TProp> property)
            where TCommand : ICanExecuteChange
        {
            property.ValueChanged += value => command.OnCanExecuteChanged();
            return command;
        }
        
        
        public static TCommand UpdateOn<TCommand>(this TCommand command, INotifyCollectionChanged property)
            where TCommand : ICanExecuteChange
        {
            property.CollectionChanged += (s, a) => command.OnCanExecuteChanged();
            return command;
        }
    }
}