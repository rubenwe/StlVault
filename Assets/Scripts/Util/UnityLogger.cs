namespace StlVault.Util
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
                _unityLogger.Log(string.Format(message, args));
            }
        }

        public void Trace(string message, params object[] args)
        {
            if (LogLevel >= LogLevel.Trace)
            {
                _unityLogger.Log(string.Format(message, args));
            }
        }

        public void Info(string message, params object[] args)
        {
            if (LogLevel >= LogLevel.Info)
            {
                _unityLogger.Log(string.Format(message, args));
            }
        }

        public void Warn(string message, params object[] args)
        {
            if (LogLevel >= LogLevel.Warn)
            {
                _unityLogger.LogWarning(message, string.Format(message, args));
            }
        }

        public void Error(string message, params object[] args)
        {
            if (LogLevel >= LogLevel.Error)
            {
                _unityLogger.LogError(message, string.Format(message, args));
            }
        }
    }
}