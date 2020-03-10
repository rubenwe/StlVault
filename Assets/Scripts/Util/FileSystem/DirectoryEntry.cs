using System.Collections.Generic;

namespace StlVault.Util.FileSystem
{
    /// <summary>
    /// Symbolizes a <c>DirectoryEntry</c> in the <seealso cref="FileSystem"/>.
    /// </summary>
    public abstract class DirectoryEntry : Entry
    {
        /// <summary>
        /// Gets the entry for the specified element.
        /// </summary>
        /// <param name="relativePath">The relative path to an element.</param>
        /// <returns>Entry for specified path.</returns>
        public abstract Entry GetEntry(string relativePath);

        /// <summary>
        /// Tries to get the entry for the specified element.
        /// </summary>
        /// <param name="relativePath">The relative path to an element.</param>
        /// <returns>Entry for specified path.</returns>
        public abstract Entry TryGetEntry(string relativePath);

        /// <summary>
        /// Gets the specified entries in this directory.
        /// </summary>
        /// <param name="recursive">if set to <c>true</c> entries will be read [recursively].</param>
        /// <returns>List of Entries matching given criteria.</returns>
        public abstract IEnumerable<Entry> GetEntries(bool recursive = false);

        /// <summary>
        /// Gets the specified entries in this directory.
        /// </summary>
        /// <param name="pattern">The pattern to match entry names against.</param>
        /// <param name="recursive">if set to <c>true</c> entries will be read [recursively].</param>
        /// <returns>List of Entries matching given criteria.</returns>
        public abstract IEnumerable<Entry> GetEntries(string pattern, bool recursive = false);

        /// <summary>
        /// Gets the specified entries in this directory.
        /// </summary>
        /// <param name="type">The type of Entry to get.</param>
        /// <param name="recursive">if set to <c>true</c> entries will be read [recursively].</param>
        /// <returns>List of Entries matching given criteria.</returns>
        public IEnumerable<Entry> GetEntries(EntryType type, bool recursive = false)
        {
            switch (type)
            {
                case EntryType.Directory:
                    return GetDirectoryEntries(recursive);
                case EntryType.File:
                    return GetFileEntries(recursive);
                default:
                    return GetEntries(recursive);
            }
        }

        /// <summary>
        /// Gets the specified entries in this directory.
        /// </summary>
        /// <param name="pattern">The pattern to match entry names against.</param>
        /// <param name="type">The type of Entry to get.</param>
        /// <param name="recursive">if set to <c>true</c> entries will be read [recursively].</param>
        /// <returns>List of Entries matching given criteria.</returns>
        public IEnumerable<Entry> GetEntries(string pattern, EntryType type, bool recursive = false)
        {
            switch (type)
            {
                case EntryType.Directory:
                    return GetDirectoryEntries(pattern, recursive);
                case EntryType.File:
                    return GetFileEntries(pattern, recursive);
                default:
                    return GetEntries(pattern, recursive);
            }
        }

        /// <summary>
        /// Gets the specified directory entries in this directory. (<seealso cref="DirectoryEntry"/>).
        /// </summary>
        /// <param name="recursive">if set to <c>true</c> entries will be read [recursively].</param>
        /// <returns>
        /// List of DirectoryEntries matching given criteria.
        /// </returns>
        public abstract IEnumerable<DirectoryEntry> GetDirectoryEntries(bool recursive = false);
        
        /// <summary>
        /// Gets the specified directory entries in this directory. (<seealso cref="DirectoryEntry"/>).
        /// </summary>
        /// <param name="pattern">The pattern to match entry names against.</param>
        /// <param name="recursive">if set to <c>true</c> entries will be read [recursively].</param>
        /// <returns>
        /// List of DirectoryEntries matching given criteria.
        /// </returns>
        public abstract IEnumerable<DirectoryEntry> GetDirectoryEntries(string pattern, bool recursive = false);

        /// <summary>
        /// Gets the specified file entries in this directory. (<seealso cref="FileEntry"/>).
        /// </summary>
        /// <param name="recursive">if set to <c>true</c> entries will be read [recursively].</param>
        /// <returns>
        /// List of FileEntries matching given criteria.
        /// </returns>
        public abstract IEnumerable<FileEntry> GetFileEntries(bool recursive = false);

        /// <summary>
        /// Gets the specified file entries in this directory. (<seealso cref="FileEntry"/>).
        /// </summary>
        /// <param name="pattern">The pattern to match entry names against.</param>
        /// <param name="recursive">if set to <c>true</c> entries will be read [recursively].</param>
        /// <returns>
        /// List of FileEntries matching given criteria.
        /// </returns>
        public abstract IEnumerable<FileEntry> GetFileEntries(string pattern, bool recursive = false);

        /// <summary>
        /// Gets the List of file and/or directory paths within this Directory.
        /// </summary>
        /// <param name="recursive">if set to <c>true</c> the Listing will be [recursive].</param>
        /// <returns>
        /// List of Paths relative to the file system root.
        /// </returns>
        public abstract IEnumerable<string> GetListing(bool recursive = false);

        /// <summary>
        /// Gets the List of file and/or directory paths within this Directory.
        /// </summary>
        /// <param name="type">The EntryType of the requested Paths.</param>
        /// <param name="recursive">if set to <c>true</c> the Listing will be [recursive].</param>
        /// <returns>
        /// List of Paths relative to the file system root.
        /// </returns>
        public abstract IEnumerable<string> GetListing(EntryType type, bool recursive = false);

        /// <summary>
        /// Gets the List of file and/or directory paths within this Directory.
        /// </summary>
        /// <param name="pattern">The pattern the returned paths have to match.</param>
        /// <param name="recursive">if set to <c>true</c> the Listing will be [recursive].</param>
        /// <returns>
        /// List of Paths relative to the file system root.
        /// </returns>
        public abstract IEnumerable<string> GetListing(string pattern, bool recursive = false);

        /// <summary>
        /// Gets the List of file and/or directory paths within this Directory.
        /// </summary>
        /// <param name="pattern">The pattern the returned paths have to match.</param>
        /// <param name="type">The EntryType of the requested Paths.</param>
        /// <param name="recursive">if set to <c>true</c> the Listing will be [recursive].</param>
        /// <returns>
        /// List of Paths relative to the file system root.
        /// </returns>
        public abstract IEnumerable<string> GetListing(string pattern, EntryType type, bool recursive = false);
    }
}