using System;
using System.Threading;

namespace StlVault.Util
{
    internal static class ReaderWriterLockSlimExtensions
    {
        public static void Write(this ReaderWriterLockSlim rwLock, Action writeAction)
        {
            try
            {
                rwLock.EnterWriteLock();
                writeAction.Invoke();
            }
            finally
            {
                rwLock.ExitWriteLock();
            }
        }
        
        public static void Read(this ReaderWriterLockSlim rwLock, Action readAction)
        {
            try
            {
                rwLock.EnterReadLock();
                readAction.Invoke();
            }
            finally
            {
                rwLock.ExitReadLock();
            }
        }
        
        public static T Read<T>(this ReaderWriterLockSlim rwLock, Func<T> readAction)
        {
            try
            {
                rwLock.EnterReadLock();
                return readAction.Invoke();
            }
            finally
            {
                rwLock.ExitReadLock();
            }
        }
    }
}