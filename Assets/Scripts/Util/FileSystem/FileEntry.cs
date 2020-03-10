using System;
using System.IO;

namespace StlVault.Util.FileSystem
{
    /// <summary>
    /// Symbolizes a <c>FileEntry</c> in the <seealso cref="FileSystem"/>.
    /// </summary>
    public abstract class FileEntry : Entry 
    {
        /// <summary>
        /// Tries to get the contents of the file represented by this entry as stream.
        /// </summary>
        /// <returns>Stream of file contents.</returns>
        public Stream TryGetFile()
        {
            var s = TryGetStream();
            if(s == null)
            {
                return null;
            }

            var ss = Stream.Synchronized(s);
            try
            {
                ss.Position = 0;
            }
            catch
            {
                throw new InvalidOperationException("Stream of file could not be read properly.");
            }

            return ss;
        }

        /// <summary>
        /// Gets the contents of the file represented by this entry as stream.
        /// </summary>
        /// <returns>Stream of file contents.</returns>
        public Stream GetFile()
        {
            var stream = TryGetFile();
            if(stream == null)
            {
                throw new InvalidOperationException(string.Format(
                    "Could not read from file `{0}{2}{1}`", FileSystem.RootPath, Path, "/"));
            }

            return stream;
        }

        /// <summary>
        /// Gets the contents of the file represented by this entry as stream.
        /// </summary>
        /// <returns>Stream of file contents.</returns>
        protected abstract Stream TryGetStream();
    }
}