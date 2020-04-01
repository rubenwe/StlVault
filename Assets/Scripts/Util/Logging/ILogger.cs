using JetBrains.Annotations;

namespace StlVault.Util.Logging
{
    public interface ILogger
    {
        [StringFormatMethod("message")]
        void Debug(string message, params object[] args);

        [StringFormatMethod("message")]
        void Trace(string message, params object[] args);

        [StringFormatMethod("message")]
        void Info(string message, params object[] args);
        
        [StringFormatMethod("message")]
        void Warn(string message, params object[] args);

        [StringFormatMethod("message")]
        void Error(string message, params object[] args);
    }
}