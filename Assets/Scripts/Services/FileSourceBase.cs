using System.Collections.Generic;
using System.Threading.Tasks;
using StlVault.Config;
using StlVault.Util;

namespace StlVault.Services
{
    internal abstract class FileSourceBase : IFileSource
    {
        public BindableProperty<FileSourceState> State { get; } = new BindableProperty<FileSourceState>();
        public abstract FileSourceConfig Config { get; }
        public abstract string DisplayName { get; }
        
        protected IFileSourceSubscriber Subscriber { get; private set; }
        
        public void Subscribe(IFileSourceSubscriber subscriber)
        {
            Subscriber = subscriber;
        }
        
        public abstract Task InitializeAsync();
        public abstract Task<byte[]> GetFileBytesAsync(string resourcePath);
        public abstract IReadOnlyCollection<string> GetTags(string resourcePath);
        public abstract void Dispose();
    }
}