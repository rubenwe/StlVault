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
        private static class Lock<T>
        {
            // ReSharper disable once StaticMemberInGenericType
            public static readonly object SyncRoot = new object();
        }
        
        public Task<T> LoadAsyncOrDefault<T>() where T : class, new()
        {
            return Task.Run(LoadOrDefault<T>);
        }

        public T LoadOrDefault<T>() where T : class, new()
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
            catch
            {
                return new T();
            }
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
                    UnityLogger.Instance.Error("Could not write settings to {0}: {1}", jsonFileName, ex.Message);
                }
            });

            string Throw() => throw new InvalidDataException($"Could not get directory from file name {jsonFileName}");
        }
    }
}