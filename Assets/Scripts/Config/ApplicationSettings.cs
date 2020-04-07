using StlVault.Util;
using StlVault.Util.Logging;

namespace StlVault.Config
{
    internal class ApplicationSettings
    {
        public ushort UiScalePercent { get; set; } = 100;
        public LogLevel LogLevel { get; set; } = LogLevel.Info;
        public ushort ImportParallelism { get; set; } = 2;
        public ushort PreviewJpegQuality { get; set; } = 70;
        public ushort PreviewResolution { get; set; } = 10;
        public ushort ScrollSensitivity { get; set; } = 150;
    }
}