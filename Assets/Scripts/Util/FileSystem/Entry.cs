using System;
using System.Globalization;

namespace StlVault.Util.FileSystem
{
    /// <summary>
    /// Symbolizes an entry in a <seealso cref="FileSystem"/>.
    /// </summary>
    public abstract class Entry : IEquatable<Entry>, IComparable, IComparable<Entry>, IDisposable
    {
        /// <summary>
        /// Gets EntryType.
        /// </summary>
        /// <value>
        /// The EntryType.
        /// </value>
        public EntryType Type
        {
            get;
            protected set;
        }

        /// <summary>
        /// Relative Path in the FileSystem.
        /// </summary>
        /// <value>
        /// The path.
        /// </value>
        public string Path
        {
            get;
            protected set;
        }

        /// <summary>
        /// The FileSystem this <c>Entry</c> resides in.
        /// </summary>
        /// <value>
        /// The file system.
        /// </value>
        public FileSystem FileSystem
        {
            get;
            protected set;
        }

        private string _name;

        /// <summary>
        /// The File Name of this Entry.
        /// </summary>
        /// <value>
        /// The File Name.
        /// </value>
        public string Name
        {
            get => _name;
            protected set
            {
                var separatorChar = System.IO.Path.DirectorySeparatorChar;
                var ds = separatorChar.ToString(CultureInfo.InvariantCulture);
                
                if (value.EndsWith(ds))
                {
                    value = value.Remove(value.Length - 1);
                }

                if (value.Contains(ds))
                {
                    value = value.Substring(value.LastIndexOf(ds, StringComparison.Ordinal) + 1);
                }

                _name = value;
            }
        }

        /// <summary>
        /// Gets the last write time.
        /// </summary>
        /// <value>
        /// The last write time.
        /// </value>
        public abstract DateTime LastWriteTime { get; }

        /// <summary>
        /// Gets the creation time.
        /// </summary>
        /// <value>
        /// The creation time.
        /// </value>
        public abstract DateTime CreationTime { get; }

        /// <summary>
        /// Gets the last access time.
        /// </summary>
        /// <value>
        /// The last access time.
        /// </value>
        public abstract DateTime LastAccessTime { get; }

        /// <summary>
        /// Compares the specified Entrys based on
        /// their Path.
        /// </summary>
        /// <param name="a">Entry A.</param>
        /// <param name="b">Enty B.</param>
        /// <returns>Integer. Why U No Read?</returns>
        public static int Compare(Entry a, Entry b)
        {
            return string.Compare(a.Path, b.Path, StringComparison.Ordinal);
        }

        /// <summary>
        /// Gibt an, ob das aktuelle Objekt einem anderen Objekt des gleichen Typs entspricht.
        /// </summary>
        /// <returns>
        /// true, wenn das aktuelle Objekt gleich dem <paramref name="other"/>-Parameter ist, andernfalls false.
        /// </returns>
        /// <param name="other">Ein Objekt, das mit diesem Objekt verglichen werden soll.
        ///                 </param>
        public bool Equals(Entry other)
        {
            if (other == null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            return Path.Equals(other.Path);
        }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object"/> is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object"/> to compare with this instance.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="System.Object"/> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="T:System.NullReferenceException">The <paramref name="obj"/> parameter is null.</exception>
        public override bool Equals(object obj)
        {
            var other = obj as Entry;
            return other != null && Equals(other);
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode()
        {
            return Path != null ? Path.GetHashCode() : 0;
        }

        /// <summary>
        /// Führt anwendungsspezifische Aufgaben durch, die mit der Freigabe, 
        /// der Zurückgabe oder dem Zurücksetzen von nicht verwalteten Ressourcen zusammenhängen.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        public virtual void Dispose()
        {
            FileSystem = null;
        }

        /// <summary>
        /// Vergleicht das aktuelle Objekt mit einem anderen Objekt desselben Typs.
        /// </summary>
        /// <returns>
        /// Eine 32-Bit-Ganzzahl mit Vorzeichen, die die relative Reihenfolge der verglichenen Objekte angibt. Der Rückgabewert hat folgende Bedeutung:
        ///                     Wert 
        ///                     Bedeutung 
        ///                     Kleiner als 0 (null)
        ///                     Dieses Objekt ist kleiner als der <paramref name="other"/>-Parameter.
        ///                     0 (null)
        ///                     Dieses Objekt ist gleich <paramref name="other"/>. 
        ///                     Größer als 0 (null)
        ///                     Dieses Objekt ist größer als <paramref name="other"/>. 
        /// </returns>
        /// <param name="other">Ein Objekt, das mit diesem Objekt verglichen werden soll.</param>
        public int CompareTo(Entry other)
        {
            if(other == null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            return string.Compare(Path, other.Path, StringComparison.Ordinal);
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return Path;
        }

        /// <summary>
        /// Vergleicht die aktuelle Instanz mit einem anderen Objekt vom selben Typ und gibt eine ganze Zahl zurück, die angibt, ob die aktuelle Instanz in der Sortierreihenfolge vor oder nach dem anderen Objekt oder an derselben Position auftritt.
        /// </summary>
        /// <returns>
        /// Eine 32-Bit-Ganzzahl mit Vorzeichen, die die relative Reihenfolge der verglichenen Objekte angibt. Der Rückgabewert hat folgende Bedeutung: 
        ///                     Wert 
        ///                     Bedeutung 
        ///                     Kleiner als 0 
        ///                     Diese Instanz ist kleiner als <paramref name="obj"/>. 
        ///                     0 
        ///                     Diese Instanz ist gleich <paramref name="obj"/>. 
        ///                     Größer als 0 
        ///                     Diese Instanz ist größer als <paramref name="obj"/>. 
        /// </returns>
        /// <param name="obj">Ein Objekt, das mit dieser Instanz verglichen werden soll.</param>
        /// <exception cref="T:System.ArgumentException"><paramref name="obj"/> hat nicht denselben Typ wie diese Instanz.</exception>
        /// <filterpriority>2</filterpriority>
        public int CompareTo(object obj)
        {
            var other = obj as Entry;
            if(obj == null)
            {
                throw new ArgumentException("Obj is not of the same time as this instance.", nameof(obj));
            }

            return CompareTo(other);
        }
    }
}
