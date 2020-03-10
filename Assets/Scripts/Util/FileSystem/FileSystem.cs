using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace StlVault.Util.FileSystem
{
    /// <summary>
    /// Abstract definition of a FileSystem.
    /// </summary>
    public abstract class FileSystem : IDisposable
    {
        /// <summary>
        /// RootPath of the FileSystem.
        /// </summary>
        /// <value>
        /// The root path.
        /// </value>
        public string RootPath
        {
            get;
            protected set;
        }
        
        /// <summary>
        /// Displays whether this instance is writable.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is writable; otherwise, <c>false</c>.
        /// </value>
        public bool IsWritable
        {
            get;
            protected set;
        }

        /// <summary>
        /// Displays whether this instance is readable.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is readable; otherwise, <c>false</c>.
        /// </value>
        public bool IsReadable
        {
            get;
            protected set;
        }

        /// <summary>
        /// Gets the List of file and directory paths within the RootPath of the file system.
        /// </summary>        
        /// <param name="recursive">if set to <c>true</c> the Listing will be [recursive].</param>
        /// <returns>List of Paths relative to the file system root.</returns>
        public abstract IEnumerable<string> GetListing(bool recursive = false);

        /// <summary>
        /// Gets the List of file and directory paths within the file system.
        /// </summary>
        /// <param name="relativePath">The relative path in the file system from which to list.</param>
        /// <param name="recursive">if set to <c>true</c> the Listing will be [recursive].</param>
        /// <returns>List of Paths relative to the file system root.</returns>
        public abstract IEnumerable<string> GetListing(string relativePath, bool recursive = false);

        /// <summary>
        /// Gets the List of file and/or directory paths within the file system.
        /// </summary>
        /// <param name="relativePath">The relative path in the file system from which to list.</param>
        /// <param name="type">The EntryType of the requested Paths.</param>
        /// <param name="recursive">if set to <c>true</c> the Listing will be [recursive].</param>
        /// <returns>List of Paths relative to the file system root.</returns>
        public abstract IEnumerable<string> GetListing(string relativePath, EntryType type, bool recursive = false);

        /// <summary>
        /// Gets the List of file and directory paths within the file system.
        /// </summary>
        /// <param name="relativePath">The relative path in the file system from which to list.</param>
        /// <param name="pattern">The pattern the returned paths have to match.</param>
        /// <param name="recursive">if set to <c>true</c> the Listing will be [recursive].</param>
        /// <returns>List of Paths relative to the file system root.</returns>
        public abstract IEnumerable<string> GetListing(string relativePath, string pattern, bool recursive = false);

        /// <summary>
        /// Gets the List of file and/or directory paths within the file system.
        /// </summary>
        /// <param name="relativePath">The relative path in the file system from which to list.</param>
        /// <param name="pattern">The pattern the returned paths have to match.</param>
        /// <param name="type">The EntryType of the requested Paths.</param>
        /// <param name="recursive">if set to <c>true</c> the Listing will be [recursive].</param>
        /// <returns>List of Paths relative to the file system root.</returns>
        public abstract IEnumerable<string> GetListing(string relativePath, string pattern, EntryType type, bool recursive = false);

        /// <summary>
        /// Gets the file contents as stream.
        /// </summary>
        /// <param name="relativePath">Relative path to the file.</param>
        /// <param name="returnUnsynchronized">if set to <c>true</c> returns unsynchronized Stream.</param>
        /// <returns>Stream of requested File</returns>
        /// <exception cref="System.ArgumentException">Thrown if the is no readable file at the specified location.</exception>
        public Stream GetFile(string relativePath, bool returnUnsynchronized = false)
        {
            var stream = TryGetFile(relativePath, returnUnsynchronized);
            if(stream == null)
            {
                throw new ArgumentException($"There is no readable file at the specified location `{relativePath}`");
            }

            return stream;
        }

        /// <summary>
        /// Tries to get the file contents as stream.
        /// </summary>
        /// <param name="relativePath">Relative path to the file.</param>
        /// <param name="returnUnsynchronized">if set to <c>true</c> returns unsynchronized Stream.</param>
        /// <returns> Stream of requested File, if it exists, else null. </returns>
        /// <exception cref="System.InvalidOperationException">Stream of file could not be read properly.</exception>
        public Stream TryGetFile(string relativePath, bool returnUnsynchronized = false)
        {
            var s = TryGetStream(relativePath);
            
            if (s == null)
            {
                return null;
            }

            if (returnUnsynchronized)
            {
                return s;
            }

            var ss = Stream.Synchronized(s);
            try
            {
                ss.Position = 0;
            }
            catch (Exception)
            {
                throw new InvalidOperationException("Stream of file could not be read properly.");
            }

            return ss;
        }

        /// <summary>
        /// Checks if the specified File exists in the <c>FileSystem</c>.
        /// Directories will be treated as Files in this case.
        /// </summary>
        /// <param name="relativePath">Relative Path to the file.</param>
        /// <returns>True if the file exists, false if it doesn't.</returns>
        public abstract bool FileExists(string relativePath);


        /// <summary>
        /// Determines whether the specified relative path is a directory.
        /// </summary>
        /// <param name="relativePath">The relative path.</param>
        /// <returns>
        ///   <c>true</c> if the specified relative path is directory; otherwise, <c>false</c>.
        /// </returns>
        public abstract bool IsDirectory(string relativePath);

        /// <summary>
        /// Gets the entry for the specified element.
        /// </summary>
        /// <param name="relativePath">The relative path to an element.</param>
        /// <returns>Entry for specified path.</returns>
        public abstract Entry GetEntry(string relativePath);

        /// <summary>
        /// Tries to the get the specified entry.
        /// </summary>
        /// <param name="relativePath">The relative path to this element.</param>
        /// <returns>Entry for specified path. Null on failure.</returns>
        public abstract Entry TryGetEntry(string relativePath);

        /// <summary>
        /// Gets the specified entries from the <c>FileSystem</c>s <c>RootPath</c>.
        /// </summary>
        /// <param name="recursive">if set to <c>true</c> entries will be read [recursively].</param>
        /// <returns>List of Entries matching given criteria.</returns>
        public abstract IEnumerable<Entry> GetEntries(bool recursive = false);

        /// <summary>
        /// Gets the specified entries.
        /// </summary>
        /// <param name="relativePath">The relative path to a directory in the <c>FileSystem</c></param>
        /// <param name="recursive">if set to <c>true</c> entries will be read [recursively].</param>
        /// <returns>List of Entries matching given criteria.</returns>
        public abstract IEnumerable<Entry> GetEntries(string relativePath, bool recursive = false);
        
        /// <summary>
        /// Gets the specified entries.
        /// </summary>
        /// <param name="relativePath">The relative path to a directory in the <c>FileSystem</c></param>
        /// <param name="pattern">The pattern to match entry names against.</param>
        /// <param name="recursive">if set to <c>true</c> entries will be read [recursively].</param>
        /// <returns>List of Entries matching given criteria.</returns>
        public abstract IEnumerable<Entry> GetEntries(string relativePath, string pattern, bool recursive = false);

        /// <summary>
        /// Gets the specified entries.
        /// </summary>
        /// <param name="relativePath">The relative path to a directory in the <c>FileSystem</c></param>
        /// <param name="type">The type of Entry to get.</param>
        /// <param name="recursive">if set to <c>true</c> entries will be read [recursively].</param>
        /// <returns>List of Entries matching given criteria.</returns>
        public IEnumerable<Entry> GetEntries(string relativePath, EntryType type, bool recursive = false)
        {
            switch (type)
            {
                case EntryType.Directory:
                    return GetDirectoryEntries(relativePath, recursive);
                case EntryType.File:
                    return GetFileEntries(relativePath, recursive);
                default:
                    return GetEntries(relativePath, recursive);
            }
        }

        /// <summary>
        /// Gets the specified entries.
        /// </summary>
        /// <param name="relativePath">The relative path to a directory in the <c>FileSystem</c></param>
        /// <param name="pattern">The pattern to match entry names against.</param>
        /// <param name="type">The type of Entry to get.</param>
        /// <param name="recursive">if set to <c>true</c> entries will be read [recursively].</param>
        /// <returns>List of Entries matching given criteria.</returns>
        public IEnumerable<Entry> GetEntries(string relativePath, string pattern, EntryType type, bool recursive = false)
        {
            switch (type)
            {
                case EntryType.Directory:
                    return GetDirectoryEntries(relativePath, pattern, recursive);
                case EntryType.File:
                    return GetFileEntries(relativePath, pattern, recursive);
                default:
                    return GetEntries(relativePath, pattern, recursive);
            }
        }

        /// <summary>
        /// Gets the specified <seealso cref="DirectoryEntry"/>.
        /// </summary>
        /// <param name="relativePath">The relative path to the directory entry in the <c>FileSystem</c></param>
        /// <returns>DirectoryEntry matching the path.</returns>
        public abstract DirectoryEntry GetDirectoryEntry(string relativePath);

        /// <summary>
        /// Tries to get the specified <seealso cref="DirectoryEntry"/>.
        /// </summary>
        /// <param name="relativePath">The relative path to the directory entry in the <c>FileSystem</c></param>
        /// <returns>DirectoryEntry matching the path or null.</returns>
        public abstract DirectoryEntry TryGetDirectoryEntry(string relativePath);

        /// <summary>
        /// Gets the specified directory entries. (<seealso cref="DirectoryEntry"/>).
        /// </summary>
        /// <param name="relativePath">The relative path to a directory in the <c>FileSystem</c></param>
        /// <param name="recursive">if set to <c>true</c> entries will be read [recursively].</param>
        /// <returns>List of DirectoryEntries matching given criteria.</returns>
        public abstract IEnumerable<DirectoryEntry> GetDirectoryEntries(string relativePath, bool recursive = false);

        /// <summary>
        /// Gets the specified directory entries. (<seealso cref="DirectoryEntry"/>).
        /// </summary>
        /// <param name="relativePath">The relative path to a directory in the <c>FileSystem</c></param>
        /// <param name="pattern">The pattern to match entry names against.</param>
        /// <param name="recursive">if set to <c>true</c> entries will be read [recursively].</param>
        /// <returns>List of DirectoryEntries matching given criteria.</returns>
        public abstract IEnumerable<DirectoryEntry> GetDirectoryEntries(string relativePath, string pattern, bool recursive = false);

        /// <summary>
        /// Gets the specified <seealso cref="FileEntry"/>.
        /// </summary>
        /// <param name="relativePath">The relative path to the <c>FileEntry</c></param>
        /// <returns>FileEntry matching given path.</returns>
        public abstract FileEntry GetFileEntry(string relativePath);

        /// <summary>
        /// Tries to get the specified <seealso cref="FileEntry"/>.
        /// </summary>
        /// <param name="relativePath">The relative path to the file entry in the <c>FileSystem</c></param>
        /// <returns>
        /// FileEntry matching the path or null.
        /// </returns>
        public abstract FileEntry TryGetFileEntry(string relativePath);

        /// <summary>
        /// Gets the specified file entries. (<seealso cref="FileEntry"/>).
        /// </summary>
        /// <param name="relativePath">The relative path to a directory in the <c>FileSystem</c></param>
        /// <param name="recursive">if set to <c>true</c> entries will be read [recursively].</param>
        /// <returns>List of FileEntries matching given criteria.</returns>
        public abstract IEnumerable<FileEntry> GetFileEntries(string relativePath, bool recursive = false);

        /// <summary>
        /// Gets the specified file entries. (<seealso cref="FileEntry"/>).
        /// </summary>
        /// <param name="relativePath">The relative path to a directory in the <c>FileSystem</c></param>
        /// <param name="pattern">The pattern to match entry names against.</param>
        /// <param name="recursive">if set to <c>true</c> entries will be read [recursively].</param>
        /// <returns>List of FileEntries matching given criteria.</returns>
        public abstract IEnumerable<FileEntry> GetFileEntries(string relativePath, string pattern, bool recursive = false);

        /// <summary>
        /// Tries to get the content of a file as stream.
        /// </summary>
        /// <param name="relativePath">The relative path to the file in the <c>FileSystem</c></param>
        /// <returns>Stream of a file, null if file doesn't exist.</returns>
        protected abstract Stream TryGetStream(string relativePath);

        /// <summary>
        /// The Mode in which the <c>FileSystem</c> is opened.
        /// </summary>
        public enum OpenMode
        {
            /// <summary>
            /// Open for reading only
            /// </summary>
            Read,
            /// <summary>
            /// Open for writing only
            /// </summary>
            Write,
            /// <summary>
            /// Open for reading an writing
            /// </summary>
            ReadAndWrite
        }

        private bool _disposed;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~FileSystem() => Dispose(false);

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;

            if (disposing)
            {
                // Dispose all resources here.
            }

            _disposed = true;
        }

        public override string ToString() => GetType().Name + ": \"" + RootPath + "\"";
    }
}
