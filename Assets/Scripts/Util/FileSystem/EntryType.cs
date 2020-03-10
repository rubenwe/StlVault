namespace StlVault.Util.FileSystem
{
    /// <summary>
    /// Symbolizes the Type of an Entry.
    /// </summary>
    public enum EntryType
    {
        /// <summary>
        /// Entry is a File.
        /// </summary>
        File,
        /// <summary>
        /// Entry is a Directory.
        /// </summary>
        Directory,
        /// <summary>
        /// Entry is either File or Directory
        /// </summary>
        FileOrDirectory
    }
}