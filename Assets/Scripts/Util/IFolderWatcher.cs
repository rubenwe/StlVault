using System;

namespace StlVault.Util
{
    internal interface IFolderWatcher : IDisposable
    {
        event EventHandler<string> FileAdded;
        event EventHandler<string> FileRemoved; 
    }
    
    
}