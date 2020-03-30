using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using StlVault.Util;

namespace StlVault.Services
{
    internal interface IFileSource : IDisposable
    {
        string DisplayName { get; }
        FileSourceConfig Config { get; }
        BindableProperty<FileSourceState> State { get; }
        void Subscribe(IFileSourceSubscriber subscriber);
        Task InitializeAsync();
        Task<byte[]> GetFileBytesAsync(string resourcePath);
        IReadOnlyList<string> GetTags(string resourcePath);
    }
}