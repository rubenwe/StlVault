using System;
using System.Net.Http;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Newtonsoft.Json;
using StlVault.Messages;
using StlVault.Util.Logging;
using StlVault.Util.Messaging;
using UnityEngine;
using ILogger = StlVault.Util.Logging.ILogger;

namespace StlVault.Services
{
    internal class UpdateChecker
    {
        private static readonly ILogger Logger = UnityLogger.Instance;
        
        private readonly IMessageRelay _relay;

        public UpdateChecker([NotNull] IMessageRelay relay)
        {
            _relay = relay ?? throw new ArgumentNullException(nameof(relay));
        }

        public async Task CheckForUpdatesAsync()
        {
            const string channelFile = "release.json";

            try
            {
                using (var client = new HttpClient())
                {
                    var response = await client.GetStringAsync($"http://stlvault.com/{channelFile}");
                    if (response == null)
                    {
                        Logger.Warn("Received empty response for {0} from stlvault.com", channelFile);
                        return;
                    }
                    
                    var info = JsonConvert.DeserializeObject<UpdateInfo>(response);
                    var currentVersion = Version.Parse(Application.version);
                    var updateVersion = Version.Parse(info.Version);
                    if (updateVersion <= currentVersion)
                    {
                        Logger.Info("Current version `{0}` is up to date with {1}.", currentVersion, channelFile);
                        return;
                    }
                    
                    var message = new RequestShowDialogMessage.UpdateAvailable
                    {
                        Version = updateVersion,
                        DownloadUrl = info.UpdateUrl,
                        Changes = info.Changes
                    };

                    _relay.Send(this, message);
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error while getting {0} from stlvault.com", channelFile);
            }
        }
    }
}