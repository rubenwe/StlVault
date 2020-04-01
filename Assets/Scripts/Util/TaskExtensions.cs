using System.Diagnostics;
using System.Threading.Tasks;
using JetBrains.Annotations;
using StlVault.Util.Logging;

namespace StlVault.Util
{
    public static class TaskExtensions
    {
        private static readonly ILogger Logger = UnityLogger.Instance;

        [StringFormatMethod("text")]
        public static async Task<T> Timed<T>(this Task<T> task, string text, params object[] formatParameters)
        {
            var sw = Stopwatch.StartNew();
            var result = await task;
            sw.Stop();

            Logger.Trace(text + $" - Took {sw.ElapsedMilliseconds}ms.", formatParameters);

            return result;
        }

        [StringFormatMethod("text")]
        public static async Task Timed(this Task task, string text, params object[] formatParameters)
        {
            var sw = Stopwatch.StartNew();
            await task;
            sw.Stop();

            Logger.Trace(text + $" - Took {sw.ElapsedMilliseconds}ms.", formatParameters);
        }
    }
}