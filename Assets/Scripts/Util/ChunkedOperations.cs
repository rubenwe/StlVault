using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace StlVault.Util
{
    public static class ChunkedOperations
    {
        [SuppressMessage("ReSharper", "MethodSupportsCancellation")]
        public static async Task ChunkedForEach<T>(
            this IEnumerable<T> enumerable, Action<T> action, CancellationToken token = default)
        {
            var sw = Stopwatch.StartNew();
            foreach (var item in enumerable)
            {
                action(item);
                if (sw.ElapsedMilliseconds < 10) continue;

                await Task.Delay(1);
                if (token.IsCancellationRequested) return;

                sw.Restart();
            }
        }

        [SuppressMessage("ReSharper", "MethodSupportsCancellation")]
        public static async Task ChunkedForEach<T>(
            this IEnumerable<T> enumerable, Func<T, Task> action, CancellationToken token = default)
        {
            var sw = Stopwatch.StartNew();
            foreach (var item in enumerable)
            {
                await action(item);
                if (sw.ElapsedMilliseconds < 10) continue;

                await Task.Delay(1);
                if (token.IsCancellationRequested) return;

                sw.Restart();
            }
        }
    }
}