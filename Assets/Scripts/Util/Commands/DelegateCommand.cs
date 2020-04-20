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

    public class DelegateCommand<T> : ICommand<T>, ICanExecuteChange
    {
        public event EventHandler CanExecuteChanged;

        private readonly Action<T> _executeAction;
        private readonly Func<T, bool> _canExecuteFunc;

        public DelegateCommand(Action<T> executeAction) : this(null, executeAction)
        {
        }
        
        public DelegateCommand(Func<T, bool> canExecuteFunc, Action<T> executeAction)
        {
            _executeAction = executeAction ?? throw new ArgumentNullException(nameof(executeAction));
            _canExecuteFunc = canExecuteFunc;
        }

        public bool CanExecute(object parameter) => CanExecute((T) parameter);
        public void Execute(object parameter) => Execute((T) parameter);

        public void OnCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }

        public bool CanExecute(T parameter)
        {
            return _canExecuteFunc?.Invoke(parameter) ?? true;
        }

        public void Execute(T parameter)
        {
            Debug.Assert(CanExecute(parameter));
            _executeAction(parameter);
        }
    }
}