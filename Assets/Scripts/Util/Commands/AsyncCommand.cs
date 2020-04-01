using System;
using System.ComponentModel;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace StlVault.Util.Commands
{
    public class AsyncCommand : IAsyncCommand
    {
        private readonly Func<object, Task> _executeFunc;
        private readonly Func<object, bool> _canExecuteFunc;
        private NotifyingTaskWrapper _execution;

        public IAsyncExecution Execution => _execution;
        public bool IsExecuting => Execution != null && Execution.IsNotCompleted;

        public AsyncCommand([NotNull] Func<object, Task> executeFunc, Func<object, bool> canExecuteFunc = null)
        {
            _executeFunc = executeFunc ?? throw new ArgumentNullException(nameof(executeFunc));
            _canExecuteFunc = canExecuteFunc;
        }

        public AsyncCommand([NotNull] Func<Task> executeFunc, Func<bool> canExecuteFunc = null)
            : this(_ => executeFunc(), canExecuteFunc.Wrap())
        {
        }

        public bool CanExecute(object parameter)
        {
            return !IsExecuting && (_canExecuteFunc?.Invoke(parameter) ?? true);
        }

        public async void Execute(object parameter)
        {
            await ExecuteAsync(parameter).ConfigureAwait(true);
        }

        public async Task ExecuteAsync(object parameter)
        {
            // The completion is used to hault the execution of the task until we can notify 
            // consumers of the property changed and can execute changed events.
            var tcs = new TaskCompletionSource<object>();
            _execution = NotifyingTaskWrapper.Create(DoExecuteAsync(tcs.Task, _executeFunc, parameter));

            NotifyExecuteChange(true);

            tcs.SetResult(null);

            // Wait for Execution to complete
            await _execution.TaskCompleted;

            NotifyExecuteChange(false);

            // Wait on task to get exceptions of faulted task
            await _execution.Task;
        }

        private void NotifyExecuteChange(bool startingExecution)
        {
            var propChanged = PropertyChanged;

            if (startingExecution)
            {
                propChanged?.Invoke(this, PropertyChangedEventArgsCache.Get(nameof(Execution)));
            }

            propChanged?.Invoke(this, PropertyChangedEventArgsCache.Get(nameof(IsExecuting)));

            OnCanExecuteChanged();
        }

        private static async Task DoExecuteAsync(Task precondition, Func<object, Task> executeAsync, object parameter)
        {
            await precondition;
            await executeAsync(parameter);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler CanExecuteChanged;

        public void OnCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}