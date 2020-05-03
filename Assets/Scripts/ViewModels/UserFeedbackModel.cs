using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Windows.Input;
using StlVault.Messages;
using StlVault.Util;
using StlVault.Util.Commands;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Directory = System.IO.Directory;
using File = System.IO.File;

namespace StlVault.ViewModels
{
    internal class UserFeedbackModel : DialogModelBase<RequestShowDialogMessage.UserFeedback>
    {
        private const string TrackerUrl = "https://github.com/rubenwe/StlVault/issues";
        
        public BindableProperty<bool> IncludeUserData { get; } = new BindableProperty<bool>(true);
        public ICommand CreateArchiveCommand { get; }
        public ICommand CreateIssueCommand { get; }
        public ICommand OpenFeatureRequestsCommand { get; }
        public ICommand JoinDiscordCommand { get; }

        public UserFeedbackModel()
        {
            CreateArchiveCommand = new DelegateCommand(CreateArchive);

            CreateIssueCommand = new DelegateCommand(() => Application.OpenURL($"{TrackerUrl}/new"));
            OpenFeatureRequestsCommand = new DelegateCommand(() => Application.OpenURL($"{TrackerUrl}?q=label%3A%22feature+request%22"));
            JoinDiscordCommand = new DelegateCommand(() => Application.OpenURL("https://discord.gg/sexQM8R"));
        }

        private void CreateArchive()
        {
            try
            {
                var tempPath = Path.GetTempPath();
                var appPath = Application.persistentDataPath;
                var configPath = Path.Combine(appPath, "Config");
                
                var crashPath = Path.Combine(tempPath, "StlVault", "StlVault", "Crashes");
                var newestCrash = Directory.Exists(crashPath)
                    ? Directory.GetDirectories(crashPath)
                        .Select(dir => new DirectoryInfo(dir))
                        .OrderByDescending(dir => dir.CreationTime)
                        .FirstOrDefault()
                    : null;

                var zipPath = Path.Combine(tempPath, $"STLVault-{DateTime.Now:yy-MM-dd-HH-mm-ss}.zip");

                using (var file = ZipFile.Open(zipPath, ZipArchiveMode.Create))
                {
                    Add(file, configPath, "ApplicationSettings.json");

                    if (newestCrash != null)
                    {
                        Add(file, crashPath, Path.Combine(newestCrash.Name, "crash.dmp"));
                        Add(file, crashPath, Path.Combine(newestCrash.Name, "error.log"));
                    }

                    if (IncludeUserData)
                    {
                        Add(file, configPath, "MetaData.zip");
                        Add(file, configPath, "ImportFolders.json");
                        Add(file, appPath, "Player.log");
                        Add(file, appPath, "Player-prev.log");
                        if (newestCrash != null)
                        {
                            Add(file, crashPath, Path.Combine(newestCrash.Name, "Player.log"));
                        }
                    }
                }

                ShowInExplorer(zipPath);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        [Conditional("UNITY_STANDALONE_WIN")]
        [Conditional("UNITY_EDITOR_WIN")]
        private static void ShowInExplorer(string zipPath)
        {
            NativeMethods.BrowseTo(zipPath);
        }

        private static void Add(ZipArchive archive, string folder, string fileName)
        {
            var sourceFilePath = Path.Combine(folder, fileName);
            if (!File.Exists(sourceFilePath)) return;
            
            using (var stream =  File.Open(sourceFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                var zipArchiveEntry = archive.CreateEntry(fileName);
                var dateTime = File.GetLastWriteTime(sourceFilePath);
                if (dateTime.Year < 1980 || dateTime.Year > 2107)
                {
                    dateTime = new DateTime(1980, 1, 1, 0, 0, 0);
                }
                
                zipArchiveEntry.LastWriteTime = dateTime;
                using (var targetStream = zipArchiveEntry.Open())
                {
                    stream.CopyTo(targetStream);
                }
            }
        }

        protected override void OnAccept()
        {
        }

        protected override void Reset(bool closing)
        {
        }
    }
}