using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace StlVault.Util.FileSystem
{
    /// <summary>
    /// Wrapper for standard file system operations.
    /// </summary>
    public sealed class HardDriveFileSystem : FileSystem
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HardDriveFileSystem"/> class.
        /// </summary>
        /// <param name="absolutePath">The absolute path to the <c>RootFolder</c>.</param>
        /// <param name="mode">The mode in which the <c>FileSystem</c> is opened.</param>
        public HardDriveFileSystem(string absolutePath, OpenMode mode = OpenMode.ReadAndWrite)
        {
            RootPath = CleanUpPath(absolutePath);
            IsReadable = mode == OpenMode.Read  || mode == OpenMode.ReadAndWrite;
            IsWritable = mode == OpenMode.Write || mode == OpenMode.ReadAndWrite;
        }

        /// <summary>
        /// Gets the List of file and directory paths within the RootPath of the file system.
        /// </summary>
        /// <param name="recursive">if set to <c>true</c> the Listing will be [recursive].</param>
        /// <returns>List of Paths relative to the file system root.</returns>
        public override IEnumerable<string> GetListing(bool recursive = false)
        {
            return GetListing(null, null, EntryType.FileOrDirectory, recursive);
        }

        /// <summary>
        /// Gets the List of file and directory paths within the file system.
        /// </summary>
        /// <param name="relativePath">The relative path in the file system from which to list.</param>
        /// <param name="recursive">if set to <c>true</c> the Listing will be [recursive].</param>
        /// <returns>List of Paths relative to the file system root.</returns>
        public override IEnumerable<string> GetListing(string relativePath, bool recursive = false)
        {
            return GetListing(relativePath, null, EntryType.FileOrDirectory, recursive);
        }

        /// <summary>
        /// Gets the List of file and/or directory paths within the file system.
        /// </summary>
        /// <param name="relativePath">The relative path in the file system from which to list.</param>
        /// <param name="type">The EntryType of the requested Paths.</param>
        /// <param name="recursive">if set to <c>true</c> the Listing will be [recursive].</param>
        /// <returns>List of Paths relative to the file system root.</returns>
        public override IEnumerable<string> GetListing(string relativePath, EntryType type, bool recursive = false)
        {
            return GetListing(relativePath, null, type, recursive);
        }

        /// <summary>
        /// Gets the List of file and directory paths within the file system.
        /// </summary>
        /// <param name="relativePath">The relative path in the file system from which to list.</param>
        /// <param name="pattern">The pattern the returned paths have to match.</param>
        /// <param name="recursive">if set to <c>true</c> the Listing will be [recursive].</param>
        /// <returns>List of Paths relative to the file system root.</returns>
        public override IEnumerable<string> GetListing(string relativePath, string pattern, bool recursive = false)
        {
            return GetListing(relativePath, pattern, EntryType.FileOrDirectory, recursive);
        }

        /// <summary>
        /// Gets the List of file and/or directory paths within the file system.
        /// </summary>
        /// <param name="relativePath">The relative path in the file system from which to list.</param>
        /// <param name="pattern">The pattern the returned paths have to match.</param>
        /// <param name="type">The EntryType of the requested Paths.</param>
        /// <param name="recursive">if set to <c>true</c> the Listing will be [recursive].</param>
        /// <returns>List of Paths relative to the file system root.</returns>
        public override IEnumerable<string> GetListing(string relativePath, string pattern, EntryType type, bool recursive = false)
        {
            if (string.IsNullOrEmpty(relativePath))
            {
                relativePath = "\\";
            }

            if (string.IsNullOrEmpty(pattern))
            {
                pattern = "*";
            }  
          
            var searchOption = recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
            var absolutePath = AbsolutePathFor(relativePath);
            var rootLength = RootPath.Length;
            var ds = Path.DirectorySeparatorChar;
            
            var list = new List<string>();
            if (type == EntryType.File || type == EntryType.FileOrDirectory)
            {
                var files = Directory.GetFiles(absolutePath, pattern, searchOption);
                list.AddRange(files.Select(file => file.Substring(rootLength)));
            }

            if (type == EntryType.Directory || type == EntryType.FileOrDirectory)
            {
                var dirs = Directory.GetDirectories(absolutePath, pattern, searchOption);
                foreach (var dir in dirs.Where(x => !string.IsNullOrEmpty(x)))
                {
                    var d = dir.Substring(rootLength);
                    if (d[d.Length - 1] != ds)
                    {
                        d = string.Concat(d, ds);
                    }

                    list.Add(d);
                }                
            }

            return list;
        }


        /// <summary>
        /// Checks if the specified File exists in the <c>FileSystem</c>.
        /// </summary>
        /// <param name="relativePath">Relative Path to the file.</param>
        /// <returns>True if the file exists, false if it doesn't.</returns>
        public override bool FileExists(string relativePath)
        {
            return FileExists(relativePath, false);
        }

        /// <summary>
        /// Checks if the specified File exists in the <c>FileSystem</c>
        /// Directories will be treated as Files in this case.
        /// </summary>
        /// <param name="relativePath">Relative Path to the file.</param>
        /// <param name="isClean">if set to <c>true</c> the path will not be cleaned.</param>
        /// <returns>True if the file exists, false if it doesn't.</returns>
        private bool FileExists(string relativePath, bool isClean)
        {
            var fes = AbsolutePathFor(relativePath, isClean);            
            return File.Exists(fes) || Directory.Exists(fes);
        }

        /// <summary>
        /// Determines whether the specified relative path is a directory.
        /// </summary>
        /// <param name="relativePath">The relative path.</param>
        /// <returns>
        /// <c>true</c> if the specified relative path is directory; otherwise, <c>false</c>.
        /// </returns>
        public override bool IsDirectory(string relativePath)
        {
            return IsDirectory(relativePath, false);
        }

        /// <summary>
        /// Determines whether the specified relative path is a directory.
        /// </summary>
        /// <param name="relativePath">The relative path.</param>
        /// <param name="isClean">if set to <c>true</c> path will not be cleaned.</param>
        /// <returns>
        ///   <c>true</c> if the specified relative path is directory; otherwise, <c>false</c>.
        /// </returns>
        private bool IsDirectory(string relativePath, bool isClean)
        {
            if (!isClean)
            {
                relativePath = CleanUpPath(relativePath);
            }

            return Directory.Exists(AbsolutePathFor(relativePath, true));
        }


        /// <summary>
        /// Gets the entry for the specified element.
        /// </summary>
        /// <param name="relativePath">The relative path to an element.</param>
        /// <returns>Entry for specified path.</returns>
        public override Entry GetEntry(string relativePath)
        {
            var entry = TryGetEntry(relativePath);
            if (entry == null)
            {
                throw new ArgumentException($"There is no Entry for the specified Path: `{relativePath}`");
            }

            return entry;
        }

        /// <summary>
        /// Tries to the get the specified entry.
        /// </summary>
        /// <param name="relativePath">The relative path to this element.</param>
        /// <returns>Entry for specified path. Null on failure.</returns>
        public override Entry TryGetEntry(string relativePath)
        {
            relativePath = CleanUpPath(relativePath);

            if (FileExists(relativePath))
            {
                if (IsDirectory(relativePath))
                {
                    return new HardDriveDirectoryEntry(this, relativePath);
                }

                return new HardDriveFileEntry(this, relativePath);
            }

            return null;
        }

        /// <summary>
        /// Gets the specified entries from the <c>FileSystem</c>s <c>RootPath</c>.
        /// </summary>
        /// <param name="recursive">if set to <c>true</c> entries will be read [recursively].</param>
        /// <returns>List of Entries matching given criteria.</returns>
        public override IEnumerable<Entry> GetEntries(bool recursive = false)
        {
            return GetEntries<Entry>(null, null, EntryType.FileOrDirectory, recursive);
        }

        /// <summary>
        /// Gets the specified entries.
        /// </summary>
        /// <param name="relativePath">The relative path to a directory in the <c>FileSystem</c></param>
        /// <param name="recursive">if set to <c>true</c> entries will be read [recursively].</param>
        /// <returns>List of Entries matching given criteria.</returns>
        public override IEnumerable<Entry> GetEntries(string relativePath, bool recursive = false)
        {
            return GetEntries<Entry>(relativePath, null, EntryType.FileOrDirectory, recursive);
        }

        /// <summary>
        /// Gets the specified entries.
        /// </summary>
        /// <param name="relativePath">The relative path to a directory in the <c>FileSystem</c></param>
        /// <param name="pattern">The pattern to match entry names against.</param>
        /// <param name="recursive">if set to <c>true</c> entries will be read [recursively].</param>
        /// <returns>List of Entries matching given criteria.</returns>
        public override IEnumerable<Entry> GetEntries(string relativePath, string pattern, bool recursive = false)
        {
            return GetEntries<Entry>(relativePath, pattern, EntryType.FileOrDirectory, recursive);
        }

        /// <summary>
        /// Gets the specified entries.
        /// </summary>
        /// <param name="relativePath">The relative path to a directory in the <c>FileSystem</c></param>
        /// <param name="pattern">The pattern to match entry names against.</param>
        /// <param name="type">The <seealso cref="EntryType"/> of the Entries to fetch.</param>
        /// <param name="recursive">if set to <c>true</c> entries will be read [recursively].</param>
        /// <returns>List of Entries matching given criteria.</returns>
        private IEnumerable<T> GetEntries<T>(string relativePath, string pattern, EntryType type, bool recursive) where T : Entry
        {
            var list = new List<T>();
            if (type == EntryType.File || type == EntryType.FileOrDirectory)
            {
                var files = GetListing(relativePath, pattern, EntryType.File, recursive);
                list.AddRange(files.Select(file => new HardDriveFileEntry(this, file) as T));
            }

            if (type == EntryType.Directory || type == EntryType.FileOrDirectory)
            {
                var dirs = GetListing(relativePath, pattern, EntryType.Directory, recursive);
                list.AddRange(dirs.Select(dir => new HardDriveDirectoryEntry(this, dir) as T));
            }

            list.Sort(Entry.Compare);

            return list;
        }


        /// <summary>
        /// Gets the specified <seealso cref="DirectoryEntry"></seealso>.
        /// </summary>
        /// <param name="relativePath">The relative path to the directory entry in the <c>FileSystem</c></param>
        /// <returns>DirectoryEntry matching the path.</returns>
        public override DirectoryEntry GetDirectoryEntry(string relativePath)
        {
            var dirEntry = TryGetDirectoryEntry(relativePath);
            if(dirEntry == null)
            {
                throw new ArgumentException($"There is no DirectoryEntry at the specified location `{relativePath}`");
            }

            return dirEntry;
        }

        /// <summary>
        /// Tries to get the specified <seealso cref="DirectoryEntry"/>.
        /// </summary>
        /// <param name="relativePath">The relative path to the directory entry in the <c>FileSystem</c></param>
        /// <returns>DirectoryEntry matching the path or null.</returns>
        public override DirectoryEntry TryGetDirectoryEntry(string relativePath)
        {
            return TryGetEntry(relativePath) as DirectoryEntry;
        }

        /// <summary>
        /// Gets the specified directory entries. (<seealso cref="DirectoryEntry"></seealso>).
        /// </summary>
        /// <param name="relativePath">The relative path to a directory in the <c>FileSystem</c></param>
        /// <param name="recursive">if set to <c>true</c> entries will be read [recursively].</param>
        /// <returns>List of DirectoryEntries matching given criteria.</returns>
        public override IEnumerable<DirectoryEntry> GetDirectoryEntries(string relativePath, bool recursive = false)
        {
            return GetEntries<DirectoryEntry>(relativePath, null, EntryType.Directory, recursive);
        }

        /// <summary>
        /// Gets the specified directory entries. (<seealso cref="DirectoryEntry"></seealso>).
        /// </summary>
        /// <param name="relativePath">The relative path to a directory in the <c>FileSystem</c></param>
        /// <param name="pattern">The pattern to match entry names against.</param>
        /// <param name="recursive">if set to <c>true</c> entries will be read [recursively].</param>
        /// <returns>List of DirectoryEntries matching given criteria.</returns>
        public override IEnumerable<DirectoryEntry> GetDirectoryEntries(string relativePath, string pattern, bool recursive = false)
        {
            return GetEntries<DirectoryEntry>(relativePath, pattern, EntryType.Directory, recursive);
        }


        /// <summary>
        /// Gets the specified <seealso cref="FileEntry"></seealso>.
        /// </summary>
        /// <param name="relativePath">The relative path to the <c>FileEntry</c></param>
        /// <returns>FileEntry matching given path.</returns>
        public override FileEntry GetFileEntry(string relativePath)
        {
            var fileEntry = TryGetFileEntry(relativePath);
            if(fileEntry == null)
            {
                throw new ArgumentException($"There is no FileEntry at the specified location `{relativePath}`");
            }

            return fileEntry;
        }

        /// <summary>
        /// Tries to get the specified <seealso cref="FileEntry"/>.
        /// </summary>
        /// <param name="relativePath">The relative path to the file entry in the <c>FileSystem</c></param>
        /// <returns>
        /// FileEntry matching the path or null.
        /// </returns>
        public override FileEntry TryGetFileEntry(string relativePath)
        {
            return TryGetEntry(relativePath) as FileEntry;
        }

        /// <summary>
        /// Gets the specified file entries. (<seealso cref="FileEntry"></seealso>).
        /// </summary>
        /// <param name="relativePath">The relative path to a directory in the <c>FileSystem</c></param>
        /// <param name="recursive">if set to <c>true</c> entries will be read [recursively].</param>
        /// <returns>List of FileEntries matching given criteria.</returns>
        public override IEnumerable<FileEntry> GetFileEntries(string relativePath, bool recursive = false)
        {
            return GetEntries<FileEntry>(relativePath, null, EntryType.File, recursive);
        }

        /// <summary>
        /// Gets the specified file entries. (<seealso cref="FileEntry"></seealso>).
        /// </summary>
        /// <param name="relativePath">The relative path to a directory in the <c>FileSystem</c></param>
        /// <param name="pattern">The pattern to match entry names against.</param>
        /// <param name="recursive">if set to <c>true</c> entries will be read [recursively].</param>
        /// <returns>List of FileEntries matching given criteria.</returns>
        public override IEnumerable<FileEntry> GetFileEntries(string relativePath, string pattern, bool recursive = false)
        {
            return GetEntries<FileEntry>(relativePath, pattern, EntryType.File, recursive);
        }


        /// <summary>
        /// Tries to get the content of a file as stream.
        /// </summary>
        /// <param name="relativePath">The relative path to the file in the <c>FileSystem</c></param>
        /// <returns>Stream of a file, null if file doesn't exist.</returns>
        protected override Stream TryGetStream(string relativePath)
        {
            return FileExists(relativePath) ? File.OpenRead(AbsolutePathFor(relativePath)) : null;
        }


        /// <summary>
        /// Gets the absolute path for a given relative path.
        /// </summary>
        /// <param name="relativePath">The relative path to get an absolute path for.</param>
        /// <param name="isClean">if set to <c>true</c> the relative path is not cleaned up.</param>
        /// <returns>Absolute path to the file specified by relative path.</returns>
        private string AbsolutePathFor(string relativePath, bool isClean = false)
        {
            return RootPath + (isClean ? relativePath : CleanUpPath(relativePath));
        }

        /// <summary>
        /// Cleans up the given path. Removes leading slash.
        /// Adds trailing slash for directories.
        /// </summary>
        /// <param name="relativePath">The relative path to clean.</param>
        /// <returns>Cleaned up path.</returns>
        private string CleanUpPath(string relativePath)
        {
            var separatorChar = Path.DirectorySeparatorChar;
            var separatorString = separatorChar.ToString(CultureInfo.InvariantCulture);
            var networkPathSlashes = separatorString + separatorString;

            var p = relativePath.Replace('/', separatorChar);
            if (p.StartsWith(separatorString) && !p.StartsWith(networkPathSlashes))
            {
                p = p.Substring(1);
            }

            if (p != string.Empty && IsDirectory(p, true) && !p.EndsWith(separatorString))
            {
                p += separatorString;
            }

            return p;
        }


        /// <summary>
        /// Represents a directory in the hard drive file system.
        /// </summary>
        private class HardDriveDirectoryEntry : DirectoryEntry
        {
            /// <summary>
            /// Lock Object
            /// </summary>
            private readonly object _lockObj = new object();

            /// <summary>
            /// Gets or sets the hard drive file system this directory exists in.
            /// </summary>
            /// <value>
            /// The hard drive file system.
            /// </value>
            private HardDriveFileSystem HardDriveFileSystem
            {
                get => (HardDriveFileSystem)FileSystem;

                set => FileSystem = value;
            }
           
            private DirectoryInfo _directoryInfo;

            /// <summary>
            /// Gets or sets the directory info of this directory.
            /// </summary>
            /// <value>
            /// The directory info.
            /// </value>
            public DirectoryInfo DirectoryInfo
            {
                get
                {
                    if (_directoryInfo == null)
                    {
                        lock (_lockObj)
                        {
                            if (_directoryInfo == null)
                            {
                                var absolutePath = HardDriveFileSystem.AbsolutePathFor(Path);
                                DirectoryInfo = new DirectoryInfo(absolutePath);
                            }
                        }
                    }

                    return _directoryInfo;
                }

                private set
                {
                    lock (_lockObj)
                    {
                        _directoryInfo = value;
                    }
                }
            }            
   

            /// <summary>
            /// Initializes a new instance of the <see cref="HardDriveDirectoryEntry"/> class.
            /// </summary>
            /// <param name="fileSystem">The file system this Entry resides in.</param>
            /// <param name="relativePath">The relative path to this entity</param>
            public HardDriveDirectoryEntry(HardDriveFileSystem fileSystem, string relativePath)
            {
                HardDriveFileSystem = fileSystem;
                Path = relativePath;                
                Type = EntryType.Directory;                
                Name = relativePath;
            }


            /// <summary>
            /// Gets the entry for the specified element.
            /// </summary>
            /// <param name="relativePath">The relative path to an element.</param>
            /// <returns>Entry for specified path.</returns>
            public override Entry GetEntry(string relativePath)
            {
                var cPath = HardDriveFileSystem.CleanUpPath(relativePath);
                return HardDriveFileSystem.GetEntry(Path + cPath);
            }

            /// <summary>
            /// Tries to get the entry for the specified element.
            /// </summary>
            /// <param name="relativePath">The relative path to an element.</param>
            /// <returns>Entry for specified path.</returns>
            public override Entry TryGetEntry(string relativePath)
            {
                var cPath = HardDriveFileSystem.CleanUpPath(relativePath);
                return HardDriveFileSystem.TryGetEntry(Path + cPath);
            }

            /// <summary>
            /// Gets the specified entries in this directory.
            /// </summary>
            /// <param name="recursive">if set to <c>true</c> entries will be read [recursively].</param>
            /// <returns>List of Entries matching given criteria.</returns>
            public override IEnumerable<Entry> GetEntries(bool recursive = false)
            {
                return HardDriveFileSystem.GetEntries(Path, recursive);
            }

            /// <summary>
            /// Gets the specified entries in this directory.
            /// </summary>
            /// <param name="pattern">The pattern to match entry names against.</param>
            /// <param name="recursive">if set to <c>true</c> entries will be read [recursively].</param>
            /// <returns>List of Entries matching given criteria.</returns>
            public override IEnumerable<Entry> GetEntries(string pattern, bool recursive = false)
            {
                return HardDriveFileSystem.GetEntries(Path, pattern, recursive);
            }

            /// <summary>
            /// Gets the specified directory entries in this directory. (<seealso cref="DirectoryEntry"></seealso>).
            /// </summary>
            /// <param name="recursive">if set to <c>true</c> entries will be read [recursively].</param>
            /// <returns>
            /// List of DirectoryEntries matching given criteria.
            /// </returns>
            public override IEnumerable<DirectoryEntry> GetDirectoryEntries(bool recursive = false)
            {
                return HardDriveFileSystem.GetDirectoryEntries(Path, recursive);
            }

            /// <summary>
            /// Gets the specified directory entries in this directory. (<seealso cref="DirectoryEntry"></seealso>).
            /// </summary>
            /// <param name="pattern">The pattern to match entry names against.</param>
            /// <param name="recursive">if set to <c>true</c> entries will be read [recursively].</param>
            /// <returns>
            /// List of DirectoryEntries matching given criteria.
            /// </returns>
            public override IEnumerable<DirectoryEntry> GetDirectoryEntries(string pattern, bool recursive = false)
            {
                return HardDriveFileSystem.GetDirectoryEntries(Path, pattern, recursive);
            }

            /// <summary>
            /// Gets the specified file entries in this directory. (<seealso cref="FileEntry"></seealso>).
            /// </summary>
            /// <param name="recursive">if set to <c>true</c> entries will be read [recursively].</param>
            /// <returns>
            /// List of FileEntries matching given criteria.
            /// </returns>
            public override IEnumerable<FileEntry> GetFileEntries(bool recursive = false)
            {
                return HardDriveFileSystem.GetFileEntries(Path, recursive);
            }

            /// <summary>
            /// Gets the specified file entries in this directory. (<seealso cref="FileEntry"></seealso>).
            /// </summary>
            /// <param name="pattern">The pattern to match entry names against.</param>
            /// <param name="recursive">if set to <c>true</c> entries will be read [recursively].</param>
            /// <returns>
            /// List of FileEntries matching given criteria.
            /// </returns>
            public override IEnumerable<FileEntry> GetFileEntries(string pattern, bool recursive = false)
            {
                return HardDriveFileSystem.GetFileEntries(Path, pattern, recursive);
            }


            /// <summary>
            /// Gets the List of file and/or directory paths within this Directory.
            /// </summary>
            /// <param name="recursive">if set to <c>true</c> the Listing will be [recursive].</param>
            /// <returns>
            /// List of Paths relative to the file system root.
            /// </returns>
            public override IEnumerable<string> GetListing(bool recursive = false)
            {
                return HardDriveFileSystem.GetListing(Path, recursive);
            }

            /// <summary>
            /// Gets the List of file and/or directory paths within this Directory.
            /// </summary>
            /// <param name="type">The EntryType of the requested Paths.</param>
            /// <param name="recursive">if set to <c>true</c> the Listing will be [recursive].</param>
            /// <returns>
            /// List of Paths relative to the file system root.
            /// </returns>
            public override IEnumerable<string> GetListing(EntryType type, bool recursive = false)
            {
                return HardDriveFileSystem.GetListing(Path, type, recursive);
            }

            /// <summary>
            /// Gets the List of file and/or directory paths within this Directory.
            /// </summary>
            /// <param name="pattern">The pattern the returned paths have to match.</param>
            /// <param name="recursive">if set to <c>true</c> the Listing will be [recursive].</param>
            /// <returns>
            /// List of Paths relative to the file system root.
            /// </returns>
            public override IEnumerable<string> GetListing(string pattern, bool recursive = false)
            {
                return HardDriveFileSystem.GetListing(Path, pattern, recursive);
            }

            /// <summary>
            /// Gets the List of file and/or directory paths within this Directory.
            /// </summary>
            /// <param name="pattern">The pattern the returned paths have to match.</param>
            /// <param name="type">The EntryType of the requested Paths.</param>
            /// <param name="recursive">if set to <c>true</c> the Listing will be [recursive].</param>
            /// <returns>
            /// List of Paths relative to the file system root.
            /// </returns>
            public override IEnumerable<string> GetListing(string pattern, EntryType type, bool recursive = false)
            {
                return HardDriveFileSystem.GetListing(Path, pattern, type, recursive);
            }

            /// <summary>
            /// Gets the last write time.
            /// </summary>
            /// <value>
            /// The last write time.
            /// </value>
            public override DateTime LastWriteTime => DirectoryInfo.LastWriteTime;

            /// <summary>
            /// Gets the creation time.
            /// </summary>
            /// <value>
            /// The creation time.
            /// </value>
            public override DateTime CreationTime => DirectoryInfo.CreationTime;

            /// <summary>
            /// Gets the last access time.
            /// </summary>
            /// <value>
            /// The last access time.
            /// </value>
            public override DateTime LastAccessTime => DirectoryInfo.LastAccessTime;
        }


        /// <summary>
        /// Represents a file in the hard drive file system.
        /// </summary>
        private class HardDriveFileEntry : FileEntry
        {
            /// <summary>
            /// Gets or sets the hard drive file system this directory exists in.
            /// </summary>
            /// <value>
            /// The hard drive file system.
            /// </value>
            private HardDriveFileSystem HardDriveFileSystem
            {
                get => (HardDriveFileSystem)FileSystem;
                set => FileSystem = value;
            }
            
            private FileInfo _fileInfo;
            
            /// <summary>
            /// Gets or sets the directory info of this directory.
            /// </summary>
            /// <value>
            /// The directory info.
            /// </value>
            private FileInfo FileInfo
            {
                get
                {
                    if (_fileInfo == null)
                    {
                        if (_fileInfo == null)
                        {
                            var absolutePath = HardDriveFileSystem.AbsolutePathFor(Path);
                            var newInfo = new FileInfo(absolutePath);
                            FileInfo = newInfo;
                        }
                    }

                    return _fileInfo;
                }
                set => _fileInfo = value;
            }
                        
            /// <summary>
            /// Initializes a new instance of the <see cref="HardDriveFileEntry"/> class.
            /// </summary>
            /// <param name="fileSystem">The file system the file resides in.</param>
            /// <param name="relativePath">The relative path to this file.</param>
            public HardDriveFileEntry(HardDriveFileSystem fileSystem, string relativePath)
            {
                Path = relativePath;
                HardDriveFileSystem = fileSystem;
                Type = EntryType.File;
                Name = relativePath;
            }
            
            /// <summary>
            /// Gets the contents of the file represented by this entry as stream.
            /// </summary>
            /// <returns>Stream of file contents.</returns>
            protected override Stream TryGetStream()
            {
                return HardDriveFileSystem.TryGetStream(Path);
            }

            /// <summary>
            /// Gets the last write time.
            /// </summary>
            /// <value>
            /// The last write time.
            /// </value>
            public override DateTime LastWriteTime => FileInfo.LastWriteTime;

            /// <summary>
            /// Gets the creation time.
            /// </summary>
            /// <value>
            /// The creation time.
            /// </value>
            public override DateTime CreationTime => FileInfo.CreationTime;

            /// <summary>
            /// Gets the last access time.
            /// </summary>
            /// <value>
            /// The last access time.
            /// </value>
            public override DateTime LastAccessTime => FileInfo.LastAccessTime;
        }
    }
}
