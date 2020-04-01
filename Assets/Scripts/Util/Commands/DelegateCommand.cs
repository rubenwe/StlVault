using System;
using System.Diagnostics;
using System.Windows.Input;

namespace StlVault.Util.Commands
{
    public class DelegateCommand : ICommand, ICanExecuteChange
    {
        public event EventHandler CanExecuteChanged;

        private readonly Action _executeAction;
        private readonly Func<bool> _canExecuteFunc;

        public DelegateCommand(Action executeAction) : this(null, executeAction)
        {
        }

        public DelegateCommand(Func<bool> canExecuteFunc, Action executeAction)
        {
            _executeAction = executeAction ?? throw new ArgumentNullException(nameof(executeAction));
            _canExecuteFunc = canExecuteFunc;
        }

        public bool CanExecute(object parameter)
        {
            return _canExecuteFunc?.Invoke() ?? true;
        }

        public void Execute(object parameter)
        {
            Debug.Assert(CanExecute(parameter));

            _executeAction();
        }

        public void OnCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public class DelegateCommand<T> : ICommand, ICanExecuteChange
    {
        public event EventHandler CanExecuteChanged;

        private readonly Action<T> _executeAction;
        private readonly Func<T, bool> _canExecuteFunc;

        public DelegateCommand(Action<T> executeAction, Func<T, bool> canExecuteFunc = null)
        {
            _executeAction = executeAction ?? throw new ArgumentNullException(nameof(executeAction));
            _canExecuteFunc = canExecuteFunc;
        }

        public bool CanExecute(object parameter)
        {
            return _canExecuteFunc?.Invoke((T) parameter) ?? true;
        }

        public void Execute(object parameter)
        {
            Debug.Assert(CanExecute(parameter));

            _executeAction((T) parameter);
        }

        public void OnCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}