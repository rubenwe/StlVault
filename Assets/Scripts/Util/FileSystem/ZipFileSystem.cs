using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Ionic.Zip;

namespace StlVault.Util.FileSystem
{
    /// <summary>
    /// The ZipFileSystem class emulates a FileSystem for a mounted ZipFile.
    /// </summary>
    public sealed class ZipFileSystem : FileSystem
    {
        /// <summary>
        /// Gets or sets a value indicating whether the archive  
        /// should be loaded and held in memory.
        /// </summary>
        /// <value>
        ///   <c>true</c> if [archive in memory]; otherwise, <c>false</c>.
        /// </value>
        private bool ArchiveInMemory { get; }
        
        private Stream _zipFileStream;
        private ZipFile _zipInfo;
        
        private const string DoubleZipPathSeparatorString = "//";
        private const string ZipPathSeparatorString = "/";
        private const char ZipPathSeparatorChar = '/';
        private const string StarPatternString = "*";

        private ZipFile ZipFileInfo
        {
            get
            {
                if (_zipInfo != null) return _zipInfo;
                if (ArchiveInMemory)
                {
                    _zipFileStream = new MemoryStream(File.ReadAllBytes(RootPath));
                    ZipFileInfo = ZipFile.Read(_zipFileStream);
                }
                else
                {
                    ZipFileInfo = ZipFile.Read(RootPath);
                }
                
                return _zipInfo;
            }
            set => _zipInfo = value;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ZipFileSystem"/> class.
        /// </summary>
        /// <param name="absolutePath">The absolute path to a zip file.</param>
        /// <param name="mode">The mode in which the ZipFileSystem will be opened.</param>
        /// <param name="copyArchiveToRamFirst">if set to <c>true</c> the archive will be copied to Memory.</param>
        public ZipFileSystem(string absolutePath, bool copyArchiveToRamFirst = false, OpenMode mode = OpenMode.Read)
        {
            if (mode != OpenMode.Read) throw new NotSupportedException("This FileSystem does not (yet) support writing.");
            if (absolutePath == null) throw new ArgumentNullException(nameof(absolutePath), "A Path needs to be specified!");
            if (!File.Exists(absolutePath)) throw new ArgumentException($"The specified File `{absolutePath}` does not exist");
            if (!ZipFile.IsZipFile(absolutePath)) throw new ArgumentException($"The specified File `{absolutePath}` is no archive");

            RootPath = absolutePath;
            ArchiveInMemory = copyArchiveToRamFirst;
            SetReadWrite(mode);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ZipFileSystem"/> class.
        /// </summary>
        /// <param name="zipFileStream">The stream of a zip file.</param>
        /// <param name="copyArchiveToRamFirst">if set to <c>true</c> the archive will be copied to Memory.</param>
        /// <param name="mode">The mode in which the ZipFileSystem will be opened.</param>
        public ZipFileSystem(Stream zipFileStream, bool copyArchiveToRamFirst = false, OpenMode mode = OpenMode.Read)
        {
            if (mode != OpenMode.Read) throw new NotSupportedException("This FileSystem does not (yet) support writing.");
            if (zipFileStream == null) throw new ArgumentNullException(nameof(zipFileStream), "The given stream must not be null.");

            RootPath = string.Empty;
            ArchiveInMemory = copyArchiveToRamFirst;
            SetReadWrite(mode);
            _zipFileStream = zipFileStream;
        }

        private void SetReadWrite(OpenMode mode)
        {
            IsReadable = mode == OpenMode.Read || mode == OpenMode.ReadAndWrite;
            IsWritable = mode == OpenMode.Write || mode == OpenMode.ReadAndWrite;
        }

        /// <summary>
        /// Checks if the specified File exists in the <c>FileSystem</c>.
        /// Directories will be treated as Files in this case.
        /// </summary>
        /// <param name="relativePath">Relative Path to the file.</param>
        /// <returns>True if the file exists, false if it doesn't.</returns>
        public override bool FileExists(string relativePath) => FileExists(relativePath, false);

        private bool FileExists(string relativePath, bool isClean)
        {
            if (!isClean)
            {
                relativePath = ZipPathForPath(relativePath);
            }

            return ZipFileInfo.ContainsEntry(relativePath) ||
                   ZipFileInfo.ContainsEntry(string.Concat(relativePath, ZipPathSeparatorString)) ||
                   relativePath == ZipPathSeparatorString;
        }

        /// <summary>
        /// Determines whether the specified relative path is a directory.
        /// </summary>
        /// <param name="relativePath">The relative path.</param>
        /// <returns>
        ///   <c>true</c> if the specified relative path is directory; otherwise, <c>false</c>.
        /// </returns>
        public override bool IsDirectory(string relativePath)
        {
            return IsDirectory(relativePath, false);
        }

        private bool IsDirectory(string relativePath, bool isClean)
        {
            if (!isClean)
            {
                relativePath = ZipPathForPath(relativePath);
            }

            var endsOnSlash = relativePath.EndsWith(ZipPathSeparatorChar);
            if (endsOnSlash && FileExists(relativePath, true))
            {
                return true;
            }

            return !endsOnSlash && FileExists(string.Concat(relativePath, ZipPathSeparatorString), true);
        }

        /// <summary>
        /// Gets the List of file and directory paths within the RootPath of the file system.
        /// </summary>
        /// <param name="recursive">if set to <c>true</c> the Listing will be [recursive].</param>
        /// <returns>
        /// List of Paths relative to the file system root.
        /// </returns>
        public override IEnumerable<string> GetListing(bool recursive = false)
        {
            return GetListing(string.Empty, StarPatternString, EntryType.FileOrDirectory, recursive);
        }

        /// <summary>
        /// Gets the List of file and directory paths within the file system.
        /// </summary>
        /// <param name="relativePath">The relative path in the file system from which to list.</param>
        /// <param name="recursive">if set to <c>true</c> the Listing will be [recursive].</param>
        /// <returns>
        /// List of Paths relative to the file system root.
        /// </returns>
        public override IEnumerable<string> GetListing(string relativePath, bool recursive = false)
        {
            return GetListing(relativePath, StarPatternString, EntryType.FileOrDirectory, recursive);
        }

        /// <summary>
        /// Gets the List of file and/or directory paths within the file system.
        /// </summary>
        /// <param name="relativePath">The relative path in the file system from which to list.</param>
        /// <param name="type">The EntryType of the requested Paths.</param>
        /// <param name="recursive">if set to <c>true</c> the Listing will be [recursive].</param>
        /// <returns>
        /// List of Paths relative to the file system root.
        /// </returns>
        public override IEnumerable<string> GetListing(string relativePath, EntryType type, bool recursive = false)
        {
            return GetListing(relativePath, StarPatternString, type, recursive);
        }

        /// <summary>
        /// Gets the List of file and directory paths within the file system.
        /// </summary>
        /// <param name="relativePath">The relative path in the file system from which to list.</param>
        /// <param name="pattern">The pattern the returned paths have to match.</param>
        /// <param name="recursive">if set to <c>true</c> the Listing will be [recursive].</param>
        /// <returns>
        /// List of Paths relative to the file system root.
        /// </returns>
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
        /// <returns>
        /// List of Paths relative to the file system root.
        /// </returns>
        public override IEnumerable<string> GetListing(string relativePath, string pattern, EntryType type,
            bool recursive = false)
        {
            return FilterIndex(relativePath, pattern, type, recursive, ProcessFileName);
        }

        /// <summary>
        /// Gets the entry for the specified element.
        /// </summary>
        /// <param name="relativePath">The relative path.</param>
        /// <returns>
        /// Entry for specified path.
        /// </returns>
        public override Entry GetEntry(string relativePath)
        {
            return GetEntry(relativePath, false);
        }

        /// <summary>
        /// Tries to the get the specified entry.
        /// </summary>
        /// <param name="relativePath">The relative path to this element.</param>
        /// <returns>Entry for specified path. Null on failure.</returns>
        public override Entry TryGetEntry(string relativePath)
        {
            return TryGetEntry(relativePath, false);
        }

        private Entry TryGetEntry(string relativePath, bool pathFromIndex)
        {
            if (!pathFromIndex)
            {
                relativePath = ZipPathForPath(relativePath);
            }

            if (!pathFromIndex && !FileExists(relativePath)) return null;

            if (IsDirectory(relativePath))
            {
                return new ZipDirectoryEntry(this, relativePath);
            }

            return new ZipFileEntry(this, relativePath);
            
        }

        private Entry GetEntry(string relativePath, bool pathFromIndex)
        {
            var entry = TryGetEntry(relativePath, pathFromIndex);
            if (entry == null)
            {
                throw new ArgumentException($"There is no Entry at the specified location `{relativePath}`");
            }

            return entry;
        }

        /// <summary>
        /// Gets the specified entries from the <c>FileSystem</c>s <c>RootPath</c>.
        /// </summary>
        /// <param name="recursive">if set to <c>true</c> entries will be read [recursively].</param>
        /// <returns>
        /// List of Entries matching given criteria.
        /// </returns>
        public override IEnumerable<Entry> GetEntries(bool recursive = false)
        {
            return GetEntries(string.Empty, recursive);
        }

        /// <summary>
        /// Gets the specified entries.
        /// </summary>
        /// <param name="relativePath">The relative path to a directory in the <c>FileSystem</c></param>
        /// <param name="recursive">if set to <c>true</c> entries will be read [recursively].</param>
        /// <returns>
        /// List of Entries matching given criteria.
        /// </returns>
        public override IEnumerable<Entry> GetEntries(string relativePath, bool recursive = false)
        {
            return GetEntries(relativePath, StarPatternString, recursive);
        }

        /// <summary>
        /// Gets the specified entries.
        /// </summary>
        /// <param name="relativePath">The relative path to a directory in the <c>FileSystem</c></param>
        /// <param name="pattern">The pattern to match entry names against.</param>
        /// <param name="recursive">if set to <c>true</c> entries will be read [recursively].</param>
        /// <returns>
        /// List of Entries matching given criteria.
        /// </returns>
        public override IEnumerable<Entry> GetEntries(string relativePath, string pattern, bool recursive = false)
        {
            return FilterIndex(relativePath, pattern, EntryType.FileOrDirectory, recursive, ProcessEntry);
        }

        /// <summary>
        /// Gets the specified <seealso cref="DirectoryEntry"/>.
        /// </summary>
        /// <param name="relativePath">The relative path to the directory entry in the <c>FileSystem</c></param>
        /// <returns>
        /// DirectoryEntry matching the path.
        /// </returns>
        public override DirectoryEntry GetDirectoryEntry(string relativePath)
        {
            var dirEntry = TryGetDirectoryEntry(relativePath);
            if (dirEntry == null)
            {
                throw new DirectoryNotFoundException(
                    $"There is no DirectoryEntry at the specified location `{relativePath}`");
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
            return TryGetDirectoryEntry(relativePath, false);
        }

        /// <summary>
        /// Gets the directory entry and allows for bypassing of checks.
        /// Could be beneficial to performance, if the paths are from the
        /// zip files index and only processed internally.
        /// </summary>
        /// <param name="relativePath">The relative path.</param>
        /// <param name="isValidDirectory">if set to <c>true</c> [is valid directory].</param>
        /// <returns>The <c>DirectoryEntry</c> matching the path.</returns>
        private DirectoryEntry TryGetDirectoryEntry(string relativePath, bool isValidDirectory)
        {
            if (!isValidDirectory)
            {
                relativePath = ZipPathForPath(relativePath);
            }

            if (!isValidDirectory && !FileExists(relativePath)) return null;
            if (!isValidDirectory && !IsDirectory(relativePath)) return null;
           
            return new ZipDirectoryEntry(this, relativePath);
            
        }

        private DirectoryEntry GetDirectoryEntry(string relativePath, bool isValidDirectory)
        {
            var dirEntry = TryGetDirectoryEntry(relativePath, isValidDirectory);
            if (dirEntry == null)
            {
                throw new DirectoryNotFoundException(
                    $"There is no DirectoryEntry at the specified location `{relativePath}`");
            }

            return dirEntry;
        }

        /// <summary>
        /// Gets the specified directory entries. (<seealso cref="DirectoryEntry"/>).
        /// </summary>
        /// <param name="relativePath">The relative path to a directory in the <c>FileSystem</c></param>
        /// <param name="recursive">if set to <c>true</c> entries will be read [recursively].</param>
        /// <returns>
        /// List of DirectoryEntries matching given criteria.
        /// </returns>
        public override IEnumerable<DirectoryEntry> GetDirectoryEntries(string relativePath, bool recursive = false)
        {
            return GetDirectoryEntries(relativePath, StarPatternString, recursive);
        }

        /// <summary>
        /// Gets the specified directory entries. (<seealso cref="DirectoryEntry"/>).
        /// </summary>
        /// <param name="relativePath">The relative path to a directory in the <c>FileSystem</c></param>
        /// <param name="pattern">The pattern to match entry names against.</param>
        /// <param name="recursive">if set to <c>true</c> entries will be read [recursively].</param>
        /// <returns>
        /// List of DirectoryEntries matching given criteria.
        /// </returns>
        public override IEnumerable<DirectoryEntry> GetDirectoryEntries(string relativePath, string pattern,
            bool recursive = false)
        {
            return FilterIndex(relativePath, pattern, EntryType.Directory, recursive, ProcessDirectoryEntry);
        }

        /// <summary>
        /// Gets the specified <seealso cref="FileEntry"/>.
        /// </summary>
        /// <param name="relativePath">The relative path to the <c>FileEntry</c></param>
        /// <returns>
        /// FileEntry matching given path.
        /// </returns>
        public override FileEntry GetFileEntry(string relativePath)
        {
            var fileEntry = TryGetFileEntry(relativePath);
            if (fileEntry == null)
            {
                throw new FileNotFoundException($"There is no FileEntry at the specified location `{relativePath}`");
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
            return TryGetFileEntry(relativePath, false);
        }

        /// <summary>
        /// Gets the file entry and allows for bypassing of checks.
        /// Could be beneficial to performance, if the paths are from the
        /// zip files index and only processed internally.
        /// </summary>
        /// <param name="relativePath">The relative path.</param>
        /// <param name="isValidFile">if set to <c>true</c> [is valid file].</param>
        /// <returns>The <c>FileEntry</c> matching the path.</returns>
        private FileEntry TryGetFileEntry(string relativePath, bool isValidFile)
        {
            if (!isValidFile)
            {
                relativePath = ZipPathForPath(relativePath);
            }

            if (isValidFile || FileExists(relativePath))
            {
                if (isValidFile || !IsDirectory(relativePath))
                {
                    return new ZipFileEntry(this, relativePath);
                }
            }

            return null;
        }


        /// <summary>
        /// Gets the file entry and allows for bypassing of checks.
        /// Could be beneficial to performance, if the paths are from the
        /// zip files index and only processed internally.
        /// </summary>
        /// <param name="relativePath">The relative path.</param>
        /// <param name="isValidFile">if set to <c>true</c> [is valid file].</param>
        /// <returns>The <c>FileEntry</c> matching the path.</returns>
        private FileEntry GetFileEntry(string relativePath, bool isValidFile)
        {
            var fileEntry = TryGetFileEntry(relativePath, isValidFile);
            if (fileEntry == null)
            {
                throw new FileNotFoundException($"There is no FileEntry at the specified location `{relativePath}`");
            }

            return fileEntry;
        }

        /// <summary>
        /// Gets the specified file entries. (<seealso cref="FileEntry"/>).
        /// </summary>
        /// <param name="relativePath">The relative path to a directory in the <c>FileSystem</c></param>
        /// <param name="recursive">if set to <c>true</c> entries will be read [recursively].</param>
        /// <returns>
        /// List of FileEntries matching given criteria.
        /// </returns>
        public override IEnumerable<FileEntry> GetFileEntries(string relativePath, bool recursive = false)
        {
            return GetFileEntries(relativePath, StarPatternString, recursive);
        }

        /// <summary>
        /// Gets the specified file entries. (<seealso cref="FileEntry"/>).
        /// </summary>
        /// <param name="relativePath">The relative path to a directory in the <c>FileSystem</c></param>
        /// <param name="pattern">The pattern to match entry names against.</param>
        /// <param name="recursive">if set to <c>true</c> entries will be read [recursively].</param>
        /// <returns>
        /// List of FileEntries matching given criteria.
        /// </returns>
        public override IEnumerable<FileEntry> GetFileEntries(string relativePath, string pattern,
            bool recursive = false)
        {
            return FilterIndex(relativePath, pattern, EntryType.File, recursive, ProcessFileEntry);
        }

        /// <summary>
        /// Tries to get the content of a file as stream.
        /// </summary>
        /// <param name="relativePath">The relative path to the file in the <c>FileSystem</c></param>
        /// <returns>Stream of a file, null if file doesn't exist.</returns>
        protected override Stream TryGetStream(string relativePath)
        {
            relativePath = ZipPathForPath(relativePath);

            Stream ms = new MemoryStream();
            
            if (!ZipFileInfo.ContainsEntry(relativePath)) return null;

            var e = ZipFileInfo[relativePath];
            if (e.IsDirectory) return null;

            e.Extract(ms);

            return ms;
        }

        private IEnumerable<TOut> FilterIndex<TOut>(
            string relativePath,
            string pattern,
            EntryType type,
            bool recursive,
            Func<ZipEntry, TOut> processEntry)
        {
            relativePath = ZipPathForPath(relativePath);
            ValidateDirectoryPath(relativePath);

            if (relativePath == ZipPathSeparatorString) relativePath = string.Empty;
            if (string.IsNullOrEmpty(pattern)) pattern = StarPatternString;

            Regex patternRegex = null;
            if (pattern != StarPatternString) patternRegex = pattern.ToRegex();
            
            foreach (var entry in ZipFileInfo.Entries)
            {
                var filePath = entry.FileName;
                if (!filePath.StartsWith(relativePath)) continue;

                var fileName = filePath.Substring(relativePath.Length);
                if (fileName == string.Empty) continue;
                if (IsRoot(recursive, fileName)) continue;
                if (!LastSegmentMatches(fileName, patternRegex)) continue;

                if (type == EntryType.FileOrDirectory || IsOfRequestedType(fileName, type))
                {
                    yield return processEntry(entry);
                }
            }
        }

        private static bool IsOfRequestedType(string fileName, EntryType type)
        {
            return type == EntryType.Directory && fileName.EndsWith(ZipPathSeparatorString)
                   || type == EntryType.File && !fileName.EndsWith(ZipPathSeparatorString);
        }

        private static bool IsRoot(bool recursive, string fileName)
        {
            return !recursive && fileName.Contains(ZipPathSeparatorString) && !IsTopLevelDir(fileName);
        }

        private static bool LastSegmentMatches(string fileName, Regex patternRegex)
        {
            if (patternRegex == null)
            {
                return true;
            }

            var lastSegment = GetLastPathSegment(fileName);
            return patternRegex.IsMatch(lastSegment);
        }

        private static string GetLastPathSegment(string fileName)
        {
            if (fileName == null) return null;
            fileName = fileName.TrimEnd(ZipPathSeparatorChar);

            if (fileName == string.Empty) return string.Empty;

            // Is last segment
            return fileName.Contains(ZipPathSeparatorString) 
                ? fileName.Substring(fileName.LastIndexOf(ZipPathSeparatorChar)) 
                : fileName;
        }

        private static string ProcessFileName(ZipEntry e)
        {
            return e.FileName.Replace(ZipPathSeparatorChar, Path.DirectorySeparatorChar);
        }

        private Entry ProcessEntry(ZipEntry e) => GetEntry(e.FileName, true);
        private DirectoryEntry ProcessDirectoryEntry(ZipEntry e) => GetDirectoryEntry(e.FileName, true);
        private FileEntry ProcessFileEntry(ZipEntry e) => GetFileEntry(e.FileName, true);

        /// <summary>
        /// Transforms a given relative path with forward or backward slashes 
        /// into a relative path with forward slashes.
        /// </summary>
        /// <param name="relativePath">The relative path.</param>
        /// <returns></returns>
        private string ZipPathForPath(string relativePath)
        {
            if (relativePath == null)
            {
                relativePath = string.Empty;
            }

            var p = relativePath.Replace('\\', ZipPathSeparatorChar);
            p = ReduceSlashes(p);

            if (p.StartsWith(ZipPathSeparatorString, StringComparison.InvariantCulture))
            {
                p = p.Substring(1);
            }

            if (IsDirectory(p, true) && !p.EndsWith(ZipPathSeparatorChar))
            {
                p = string.Concat(p, ZipPathSeparatorString);
            }

            return p;
        }

        private static string ReduceSlashes(string path)
        {
            var p = path.Replace(DoubleZipPathSeparatorString, ZipPathSeparatorString);
            return p.Contains(DoubleZipPathSeparatorString) ? ReduceSlashes(p) : p;
        }

        /// <summary>
        /// Validates the directory path.
        /// </summary>
        /// <remarks>
        /// Checks if the given Path is a directory and if that
        /// directory is contained in the loaded ZipFile.
        /// </remarks>
        /// <param name="relativePath">The relative path.</param>
        private void ValidateDirectoryPath(string relativePath)
        {
            if (!(relativePath == string.Empty || relativePath.EndsWith(ZipPathSeparatorString)))
            {
                throw new ArgumentException($"Specified Path `{relativePath}` is not valid");
            }

            if (!(relativePath == string.Empty || FileExists(relativePath, true)))
            {
                throw new ArgumentException($"Specified Path `{relativePath}` does not exist");
            }
        }

        /// <summary>
        /// Determines whether the given path is in the style of a top level directory.
        /// </summary>
        /// <remarks>
        /// This function will not check if the directory is on the top level of the zip
        /// file. Its function is to determine, if the path looks like it could be on
        /// the top level.
        /// </remarks>
        /// <param name="relativePath">The relativePath.</param>
        /// <returns>
        ///   <c>true</c> if the path looks like a top level directory; otherwise, <c>false</c>.
        /// </returns>
        private static bool IsTopLevelDir(string relativePath)
        {
            return relativePath.EndsWith(ZipPathSeparatorString) &&
                   relativePath.LastIndexOf(ZipPathSeparatorString, StringComparison.Ordinal) ==
                   relativePath.IndexOf(ZipPathSeparatorString, StringComparison.Ordinal);
        }


        private bool _disposed;

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; 
        /// <c>false</c> to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (_disposed) return;

            if (disposing)
            {
                if (_zipFileStream != null)
                {
                    _zipFileStream.Close();
                    _zipFileStream.Dispose();
                    _zipFileStream = null;
                }

                if (_zipInfo != null)
                {
                    _zipInfo.Dispose();
                    _zipInfo = null;
                }
            }

            _disposed = true;

            base.Dispose(disposing);
        }

        /// <summary>
        /// Directory Entry of a ZipFileSystem.
        /// </summary>
        private class ZipDirectoryEntry : DirectoryEntry
        {
            private ZipEntry _zipEntry;

            private ZipFileSystem ZipFileSystem
            {
                get => (ZipFileSystem) FileSystem;
                set => FileSystem = value;
            }

            public ZipDirectoryEntry(ZipFileSystem fileSystem, string relativePath)
            {
                ZipFileSystem = fileSystem;
                _zipEntry = ZipFileSystem.ZipFileInfo[relativePath];
                Path = relativePath.Replace(ZipPathSeparatorChar, System.IO.Path.DirectorySeparatorChar);
                Name = _zipEntry.FileName.Replace(ZipPathSeparatorChar, System.IO.Path.DirectorySeparatorChar);
                Type = EntryType.Directory;
            }

            public override DateTime LastWriteTime => _zipEntry.LastModified;
            public override DateTime CreationTime => _zipEntry.CreationTime;
            public override DateTime LastAccessTime => _zipEntry.AccessedTime;

            public override void Dispose()
            {
                _zipEntry = null;
                ZipFileSystem = null;
                base.Dispose();
            }

            /// <summary>
            /// Gets the entry for the specified element.
            /// </summary>
            /// <param name="relativePath">The relative path to this element.</param>
            /// <returns>
            /// Entry for specified path.
            /// </returns>
            public override Entry GetEntry(string relativePath)
            {
                var filepath = ZipFileSystem.ZipPathForPath(relativePath);
                return ZipFileSystem.GetEntry(Path + filepath);
            }

            /// <summary>
            /// Tries to get the entry for the specified element.
            /// </summary>
            /// <param name="relativePath">The relative path to an element.</param>
            /// <returns>Entry for specified path.</returns>
            public override Entry TryGetEntry(string relativePath)
            {
                var filepath = ZipFileSystem.ZipPathForPath(relativePath);
                return ZipFileSystem.TryGetEntry(Path + filepath);
            }

            /// <summary>
            /// Gets the specified entries in this directory.
            /// </summary>
            /// <param name="recursive">if set to <c>true</c> entries will be read [recursively].</param>
            /// <returns>
            /// List of Entries matching given criteria.
            /// </returns>
            public override IEnumerable<Entry> GetEntries(bool recursive = false)
            {
                return ZipFileSystem.GetEntries(Path, recursive);
            }

            /// <summary>
            /// Gets the specified entries in this directory.
            /// </summary>
            /// <param name="pattern">The pattern to match entry names against.</param>
            /// <param name="recursive">if set to <c>true</c> entries will be read [recursively].</param>
            /// <returns>
            /// List of Entries matching given criteria.
            /// </returns>
            public override IEnumerable<Entry> GetEntries(string pattern, bool recursive = false)
            {
                return ZipFileSystem.GetEntries(Path, pattern, recursive);
            }

            /// <summary>
            /// Gets the specified directory entries in this directory. (<seealso cref="DirectoryEntry"/>).
            /// </summary>
            /// <param name="recursive">if set to <c>true</c> entries will be read [recursively].</param>
            /// <returns>
            /// List of DirectoryEntries matching given criteria.
            /// </returns>
            public override IEnumerable<DirectoryEntry> GetDirectoryEntries(bool recursive = false)
            {
                return ZipFileSystem.GetDirectoryEntries(Path, recursive);
            }

            /// <summary>
            /// Gets the specified directory entries in this directory. (<seealso cref="DirectoryEntry"/>).
            /// </summary>
            /// <param name="pattern">The pattern to match entry names against.</param>
            /// <param name="recursive">if set to <c>true</c> entries will be read [recursively].</param>
            /// <returns>
            /// List of DirectoryEntries matching given criteria.
            /// </returns>
            public override IEnumerable<DirectoryEntry> GetDirectoryEntries(string pattern, bool recursive = false)
            {
                return ZipFileSystem.GetDirectoryEntries(Path, pattern, recursive);
            }

            /// <summary>
            /// Gets the specified file entries in this directory. (<seealso cref="FileEntry"/>).
            /// </summary>
            /// <param name="recursive">if set to <c>true</c> entries will be read [recursively].</param>
            /// <returns>
            /// List of FileEntries matching given criteria.
            /// </returns>
            public override IEnumerable<FileEntry> GetFileEntries(bool recursive = false)
            {
                return ZipFileSystem.GetFileEntries(Path, recursive);
            }

            /// <summary>
            /// Gets the specified file entries in this directory. (<seealso cref="FileEntry"/>).
            /// </summary>
            /// <param name="pattern">The pattern to match entry names against.</param>
            /// <param name="recursive">if set to <c>true</c> entries will be read [recursively].</param>
            /// <returns>
            /// List of FileEntries matching given criteria.
            /// </returns>
            public override IEnumerable<FileEntry> GetFileEntries(string pattern, bool recursive = false)
            {
                return ZipFileSystem.GetFileEntries(Path, pattern, recursive);
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
                return ZipFileSystem.GetListing(Path, recursive);
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
                return ZipFileSystem.GetListing(Path, pattern, recursive);
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
                return ZipFileSystem.GetListing(Path, type, recursive);
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
                return ZipFileSystem.GetListing(Path, pattern, type, recursive);
            }
        }

        /// <summary>
        /// File Entry of a ZipFileSystem.
        /// </summary>
        private class ZipFileEntry : FileEntry
        {
            private ZipEntry _zipEntry;

            private ZipFileSystem ZipFileSystem
            {
                get => (ZipFileSystem) FileSystem;
                set => FileSystem = value;
            }

            public ZipFileEntry(ZipFileSystem fileSystem, string relativePath)
            {
                ZipFileSystem = fileSystem;
                _zipEntry = ZipFileSystem.ZipFileInfo[relativePath];
                Path = relativePath.Replace(ZipPathSeparatorChar, System.IO.Path.DirectorySeparatorChar);
                Name = _zipEntry.FileName.Replace(ZipPathSeparatorChar, System.IO.Path.DirectorySeparatorChar);
                Type = EntryType.File;
            }

            /// <summary>
            /// Gets the contents of the file represented by this entry as a stream.
            /// </summary>
            /// <returns>Stream of FileContents.</returns>
            protected override Stream TryGetStream()
            {
                return ZipFileSystem.TryGetStream(ZipFileSystem.ZipPathForPath(Path));
            }

            public override DateTime LastWriteTime => _zipEntry.LastModified;

            public override DateTime CreationTime => _zipEntry.CreationTime;

            public override DateTime LastAccessTime => _zipEntry.AccessedTime;

            public override void Dispose()
            {
                ZipFileSystem = null;
                _zipEntry = null;
                base.Dispose();
            }
        }
    }
}