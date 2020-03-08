using System;
using JetBrains.Annotations;

namespace StlVault.Util
{
    public static class Disposable
    {
        public static IDisposable FromAction(Action onDisposeAction)
        {
            return new ActionDisposable(onDisposeAction);
        }

        private class ActionDisposable : IDisposable
        {
            private readonly Action _onDisposeCallback;

            public ActionDisposable([NotNull] Action onDisposeCallback)
            {
                _onDisposeCallback = onDisposeCallback ?? throw new ArgumentNullException(nameof(onDisposeCallback));
            }

            public void Dispose()
            {
                _onDisposeCallback.Invoke();
            }
        }
    }
}