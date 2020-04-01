using StlVault.Util.Logging;

namespace StlVault.Config
{
    internal class ApplicationSettings
    {
        public ushort UiScalePercent { get; set; } = 100;
        public LogLevel LogLevel { get; set; } = LogLevel.Info;
        
        public ushort ImportParallelism { get; set; } = 2;
    }
}