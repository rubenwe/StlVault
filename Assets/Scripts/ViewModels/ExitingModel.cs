using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using StlVault.Messages;
using StlVault.Services;
using StlVault.Util.Logging;

namespace StlVault.ViewModels
{
    internal class ExitingModel : DialogModelBase<RequestShowDialogMessage.ExitingDialog>
    {
        private static readonly ILogger Logger = UnityLogger.Instance;
        private readonly Library _library;
        private readonly Action _onReadyToExit;

        public ExitingModel([NotNull] Library library, [NotNull] Action onReadyToExit)
        {
            _library = library ?? throw new ArgumentNullException(nameof(library));
            _onReadyToExit = onReadyToExit ?? throw new ArgumentNullException(nameof(onReadyToExit));
        }
        
        protected override void OnAccept()
        {
        }

        protected override void Reset()
        {
        }

        protected override async void OnShown(RequestShowDialogMessage.ExitingDialog message)
        {
            await Task.Delay(100);

            Logger.Debug("Starting to store metadata...");
            await _library.StoreChangesAsync();
            Logger.Debug("Stored metadata...");
            
            await Task.Delay(100);
            
            _onReadyToExit.Invoke();
        }
    }
}