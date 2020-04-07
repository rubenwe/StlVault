using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Ionic.Zip;
using Ionic.Zlib;
using Newtonsoft.Json;
using StlVault.Services;
using StlVault.Util.Logging;

namespace StlVault.Views
{
    internal class AppDataConfigStore : IConfigStore
    {
        private static readonly ILogger Logger = UnityLogger.Instance;

        private static readonly JsonSerializerSettings JsonSerializerSettings 
            = new JsonSerializerSettings {DefaultValueHandling = DefaultValueHandling.Ignore};

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
                            using (var zipFile = ZipFile.Read(zipName))
                            {
                                var ms = new MemoryStream();
                                zipFile[innerName].Extract(ms);
                                text = Encoding.UTF8.GetString(ms.GetBuffer());
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

        private static string GetFileNameForConfig<T>()
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var typeName = typeof(T).Name.Replace("ConfigFile", string.Empty);
            return Path.Combine(appData, "StlVault", "Config", typeName + ".json");
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
                            using (var file = new ZipFile(Encoding.UTF8))
                            {
                                file.CompressionLevel = CompressionLevel.BestCompression;
                                file.CompressionMethod = CompressionMethod.BZip2;
                                file.AddEntry(innerName, json);
                                file.Save(zipName);
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