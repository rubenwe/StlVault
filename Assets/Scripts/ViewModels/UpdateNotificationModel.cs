using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using StlVault.Config;
using StlVault.Messages;
using StlVault.Services;
using StlVault.Util;
using UnityEngine;

namespace StlVault.ViewModels
{
    internal class UpdateNotificationModel : DialogModelBase<RequestShowDialogMessage.UpdateAvailable>
    {
        private readonly IConfigStore _store;
        private RequestShowDialogMessage.UpdateAvailable _details;
        private Version _skippedVersion;

        public BindableProperty<Version> CurrentVersion { get; } = new BindableProperty<Version>();
        public BindableProperty<Version> UpdateVersion { get; } = new BindableProperty<Version>();
        public BindableProperty<string> Changes { get; } = new BindableProperty<string>();
        
        public UpdateNotificationModel([NotNull] IConfigStore store)
        {
            _store = store ?? throw new ArgumentNullException(nameof(store));
        }

        public async Task InitializeAsync()
        {
            var updateSettings = await _store.LoadAsyncOrDefault<UpdateSettings>();
            
            _skippedVersion = updateSettings.SkippedUpdateVersion == null 
                ? Version.Parse(Application.version)
                : Version.Parse(updateSettings.SkippedUpdateVersion);
        }

        protected override bool ShouldShow(RequestShowDialogMessage.UpdateAvailable message)
        {
            return message.Version > _skippedVersion || message.UserRequested;
        }

        protected override void OnShown(RequestShowDialogMessage.UpdateAvailable message)
        {
            _details = message;
            
            CurrentVersion.Value = Version.Parse(Application.version);
            UpdateVersion.Value = message.Version;
            Changes.Value = message.Changes;
        }

        protected override void Reset(bool closing)
        {
        }

        protected override void OnAccept()
        {
            Application.OpenURL(_details.DownloadUrl);
        }

        protected override async void OnCancel()
        {
            var config = new UpdateSettings{SkippedUpdateVersion = _details.Version.ToString(3)};
            await _store.StoreAsync(config);
        }
    }
}