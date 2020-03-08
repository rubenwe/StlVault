using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

namespace StlVault.Util.Commands
{
    public class CancelableCommand : ICancelableCommand
    {
        private readonly Func<object, CancellationToken, Task> _executeFunc;
        private readonly AsyncCommand _inner;
        private CancellationTokenSource _source;
        public IAsyncCommand CancelCommand { get; }

        public IAsyncExecution Execution => _inner.Execution;
        public bool CanExecute(object parameter) => _inner.CanExecute(parameter);
        public void Execute(object parameter) => _inner.Execute(parameter);

        public event EventHandler CanExecuteChanged
        {
            add => _inner.CanExecuteChanged += value;
            remove => _inner.CanExecuteChanged -= value;
        }

        public event PropertyChangedEventHandler PropertyChanged
        {
            add => _inner.PropertyChanged += value;
            remove => _inner.PropertyChanged -= value;
        }

        public CancelableCommand(Func<CancellationToken, Task> executeFunc, Func<bool> canExecuteFunc)
            : this((_, token) => executeFunc(token), canExecuteFunc.Wrap())
        {}

        public CancelableCommand(Func<object, CancellationToken, Task> executeFunc, Func<object, bool> canExecuteFunc)
        {
            _executeFunc = executeFunc;
            _inner = new AsyncCommand(ExecuteCancelable, canExecuteFunc);
            CancelCommand = new AsyncCommand(CancelExecution, CanCancelExecution);
        }

        private async Task CancelExecution()
        {
            _source.Cancel();
            await _inner.Execution.TaskCompleted;
        }

        private bool CanCancelExecution()
        {
            return _source?.Token.IsCancellationRequested == false;
        }

        private async Task ExecuteCancelable(object arg)
        {
            _source = new CancellationTokenSource();
            await _executeFunc(arg, _source.Token);
        }

        public async Task ExecuteAsync(object parameter)
        {
            try
            {
                await _inner.ExecuteAsync(parameter);
            }
            catch (OperationCanceledException ex)
            {
                if (ex.CancellationToken != _source.Token)
                {
                    throw;
                }
            }
        }

        public void OnCanExecuteChanged()
        {
            _inner.OnCanExecuteChanged();
        }
    }
}