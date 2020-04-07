using System;
using System.Threading.Tasks;
using StlVault.Config;
using StlVault.Messages;
using StlVault.Services;
using StlVault.Util;
using StlVault.Util.Logging;

namespace StlVault.ViewModels
{
    internal class ApplicationSettingsModel : DialogModelBase<RequestShowAppSettingsDialogMessage>
    {
        private readonly IConfigStore _store;
        public ApplicationSettingsModel RuntimeSettings { get; }

        public BindableProperty<ushort> UiScalePercent { get; } = new BindableProperty<ushort>();
        public BindableProperty<ushort> ImportParallelism { get; } = new BindableProperty<ushort>();
        public BindableProperty<LogLevel> LogLevel { get; } = new BindableProperty<LogLevel>();
        public BindableProperty<ushort> PreviewResolution { get; } = new BindableProperty<ushort>();
        public BindableProperty<ushort> PreviewJpegQuality { get; } = new BindableProperty<ushort>();

        public ApplicationSettingsModel(IConfigStore store = null)
        {
            if (store == null) return;
            
            _store = store;
            RuntimeSettings = new ApplicationSettingsModel();
        }

        public async Task InitializeAsync()
        {
            var settings = await _store.LoadAsyncOrDefault<ApplicationSettings>();
            
            ApplySettings(settings);
            RuntimeSettings.ApplySettings(settings);
        }
        
        protected override async void OnAccept()
        {
            var settings = GetSettings();
            RuntimeSettings.ApplySettings(settings);
            await _store.StoreAsync(settings);
        }
        
        protected override void Reset()
        {
            var settings = RuntimeSettings.GetSettings();
            ApplySettings(settings);
        }

        private ApplicationSettings GetSettings()
        {
            return new ApplicationSettings
            {
                ImportParallelism = ImportParallelism,
                LogLevel = LogLevel,
                UiScalePercent = UiScalePercent,
                PreviewResolution = PreviewResolution,
                PreviewJpegQuality = PreviewJpegQuality
            };
        }

        private void ApplySettings(ApplicationSettings settings)
        {
            ImportParallelism.Value = settings.ImportParallelism;
            LogLevel.Value = settings.LogLevel;
            UiScalePercent.Value = settings.UiScalePercent;
            PreviewResolution.Value = settings.PreviewResolution;
            PreviewJpegQuality.Value = settings.PreviewJpegQuality;
        }
    }
}