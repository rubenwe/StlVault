using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using StlVault.Config;
using StlVault.Util;

namespace StlVault.Services
{
    internal interface IFileSource : IDisposable
    {
        string Id { get; }
        string DisplayName { get; }
        FileSourceConfig Config { get; }
        BindableProperty<FileSourceState> State { get; }
        void Subscribe(IFileSourceSubscriber subscriber);
        Task InitializeAsync();
        Task<byte[]> GetFileBytesAsync(string resourcePath);
        IReadOnlyCollection<string> GetTags(string resourcePath);
    }
}