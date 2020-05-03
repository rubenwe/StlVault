using System;
using System.IO;
using StlVault.Util.Logging;
using UnityEngine;
using ILogger = StlVault.Util.Logging.ILogger;

namespace StlVault.Services
{
    internal static class Migrator
    {
        private static readonly ILogger Logger = UnityLogger.Instance;
        
        public static void Run()
        {
            
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
            RunWindows();
#endif
        }

        public static void RunWindows()
        {
            try
            {
                var dataPath = Application.persistentDataPath;
                var versionInfoPath = Path.Combine(dataPath, "Config", "version.info");

                var version = new Version(0, 4, 0);
                if (File.Exists(versionInfoPath))
                {
                    version = Version.Parse(File.ReadAllText(versionInfoPath));
                }

                if (!Directory.Exists(dataPath))
                {
                    Directory.CreateDirectory(dataPath);
                }

                if (version < new Version(0, 5, 1))
                {
                    Logger.Info("Starting migration of config to version 0.5.1");
                    
                    var appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                    var oldPath = Path.Combine(appData, "StlVault");
                    
                    void MoveFolder(string subFolder)
                    {
                        var subPath = Path.Combine(oldPath, subFolder);
                        if (Directory.Exists(subPath))
                        {
                            Directory.Move(subPath, Path.Combine(dataPath, subFolder));
                        }
                    }

                    MoveFolder("Config");
                    MoveFolder("PreviewImages");
                    
                    Logger.Info("Migration to 0.5.1 completed.");
                }
                
                File.WriteAllText(versionInfoPath, Application.version);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Migration failed!");
            }
        }
    }
}