using System;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using StlVault.Services;
using StlVault.Util.Logging;

namespace StlVault.Views
{
    internal class AppDataConfigStore : IConfigStore
    {
        private static readonly ILogger Logger = UnityLogger.Instance;
        
        private static class Lock<T>
        {
            // ReSharper disable once StaticMemberInGenericType
            public static readonly object SyncRoot = new object();
        }
        
        public Task<T> LoadAsyncOrDefault<T>() where T : class, new()
        {
            return Task.Run(() =>
            {
                try
                {
                    var jsonFileName = GetFileNameForConfig<T>();
                
                    string text;
                    lock (Lock<T>.SyncRoot)
                    {
                        text = File.ReadAllText(jsonFileName);
                    }
                
                    return JsonConvert.DeserializeObject<T>(text);
                }
                catch (Exception ex)
                {
                    PrintException<T>(ex, "reading");
                    return new T();
                }
            });
        }

        private static string GetFileNameForConfig<T>()
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var typeName = typeof(T).Name.Replace("ConfigFile", string.Empty);
            return Path.Combine(appData, "StlVault", "Config", typeName + ".json");
        }

        public Task StoreAsync<T>(T config)
        {
            var jsonFileName = GetFileNameForConfig<T>();

            return Task.Run(() =>
            {
                try
                {
                    var dir = Path.GetDirectoryName(jsonFileName) ?? Throw();
                    Directory.CreateDirectory(dir);
                    var json = JsonConvert.SerializeObject(config, Formatting.Indented);
                    lock (Lock<T>.SyncRoot)
                    {
                        File.WriteAllText(jsonFileName, json);
                    }
                }
                catch (Exception ex)
                {
                    PrintException<T>(ex, "writing");
                }
            });

            string Throw() => throw new InvalidDataException($"Could not get directory from file name {jsonFileName}");
        }

        private static void PrintException<T>(Exception ex, string action)
        {
            if (ex is FileNotFoundException)
            {
                Logger.Info("No config file for {0} present.", typeof(T).Name);
                return;
            }
            
            Logger.Error("Error while {0} configuration file for {1}:\n{2}", action, typeof(T).Name, ex);
        }
    }
}