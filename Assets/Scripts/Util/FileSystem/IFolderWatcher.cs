using System;

namespace StlVault.Util.FileSystem
{
    internal interface IFolderWatcher : IDisposable
    {
        event EventHandler<string> FileAdded;
        event EventHandler<string> FileRemoved;
    }
}