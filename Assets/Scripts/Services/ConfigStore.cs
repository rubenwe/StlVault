using System;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Newtonsoft.Json;
using StlVault.Util.Logging;

namespace StlVault.Services
{
    internal class ConfigStore : IConfigStore
    {
        private readonly string _dataPath;
        private static readonly ILogger Logger = UnityLogger.Instance;

        private static readonly JsonSerializerSettings JsonSerializerSettings 
            = new JsonSerializerSettings {DefaultValueHandling = DefaultValueHandling.Ignore};

        public ConfigStore([NotNull] string dataPath)
        {
            _dataPath = dataPath ?? throw new ArgumentNullException(nameof(dataPath));
        }
        
        // ReSharper disable once UnusedTypeParameter
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
                        try
                        {
                            text = File.ReadAllText(jsonFileName);
                        }
                        catch (FileNotFoundException)
                        {
                            var zipName = Path.ChangeExtension(jsonFileName, "zip");
                            var innerName = Path.GetFileName(jsonFileName);
                            using (var zipFile = ZipFile.OpenRead(zipName))
                            using (var reader = new StreamReader(zipFile.GetEntry(innerName).Open()))
                            {
                                text = reader.ReadToEnd();
                            }
                        }
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

        private string GetFileNameForConfig<T>()
        {
            var typeName = typeof(T).Name.Replace("ConfigFile", string.Empty);
            return Path.Combine(_dataPath, "Config", typeName + ".json");
        }

        public Task StoreAsync<T>(T config, bool compress = false)
        {
            var jsonFileName = GetFileNameForConfig<T>();

            return Task.Run(() =>
            {
                try
                {
                    var dir = Path.GetDirectoryName(jsonFileName) ?? Throw();
                    Directory.CreateDirectory(dir);
                    var json = JsonConvert.SerializeObject(config, Formatting.Indented, JsonSerializerSettings);
                    lock (Lock<T>.SyncRoot)
                    {
                        if (!compress)
                        {
                            File.WriteAllText(jsonFileName, json);
                        }
                        else
                        {
                            var zipName = Path.ChangeExtension(jsonFileName, "zip");
                            var innerName = Path.GetFileName(jsonFileName);
                            if(File.Exists(zipName)) File.Delete(zipName);
                            using (var file = ZipFile.Open(zipName, ZipArchiveMode.Create))
                            using (var writer = new StreamWriter(file.CreateEntry(innerName).Open()))
                            {
                                writer.Write(json);
                            }
                        }
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