using System;
using System.Threading;

namespace StlVault.Util.Logging
{
    public class UnityLogger : ILogger
    {
        public static ILogger Instance { get; } = new UnityLogger();

        private readonly UnityEngine.ILogger _unityLogger;

        public static LogLevel LogLevel { get; set; }

        private UnityLogger()
        {
            _unityLogger = UnityEngine.Debug.unityLogger;
        }

        public void Debug(string message, params object[] args)
        {
            if (LogLevel >= LogLevel.Debug)
            {
                _unityLogger.Log(GetFormattedMessage(message, args));
            }
        }

        public void Trace(string message, params object[] args)
        {
            if (LogLevel >= LogLevel.Trace)
            {
                _unityLogger.Log(GetFormattedMessage(message, args));
            }
        }

        public void Info(string message, params object[] args)
        {
            if (LogLevel >= LogLevel.Info)
            {
                _unityLogger.Log(GetFormattedMessage(message, args));
            }
        }

        public void Warn(string message, params object[] args)
        {
            if (LogLevel >= LogLevel.Warn)
            {
                var formatted = GetFormattedMessage(message, args);
                _unityLogger.LogWarning(formatted, formatted);
            }
        }

        public void Warn(Exception ex, string message, params object[] args)
        {
            if (LogLevel >= LogLevel.Warn)
            {
                var formatted = GetFormattedMessage(message, args);
                _unityLogger.LogError(formatted, ex.Message);
            }
        }
        
        public void Error(string message, params object[] args)
        {
            if (LogLevel >= LogLevel.Error)
            {
                var formatted = GetFormattedMessage(message, args);
                _unityLogger.LogError(formatted, formatted);
            }
        }

        public void Error(Exception ex, string message, params object[] args)
        {
            if (LogLevel >= LogLevel.Error)
            {
                var formatted = GetFormattedMessage(message, args);
                _unityLogger.LogError(formatted, ex.Message);
            }
        }

        private static string GetFormattedMessage(string message, object[] args)
        {
            var formatted = string.Format(message, args);
            var logString = $"Thread {Thread.CurrentThread.ManagedThreadId:0000} | {formatted}";

            return logString;
        }
    }
}