using System;
using System.Threading.Tasks;
using StlVault.Config;
using StlVault.Util;

namespace StlVault.Services
{
    internal interface IImportFolder : IDisposable
    {
        Task InitializeAsync();
        BindableProperty<FolderState> FolderState { get; }
        ImportFolderConfig Config { get; }
    }
}