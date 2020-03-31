using System;
using Newtonsoft.Json;
using StlVault.Util.FileSystem;

namespace StlVault.Services
{
    internal struct BasicFileInfo : IFileInfo, IEquatable<BasicFileInfo>
    {
        public BasicFileInfo(IFileInfo file)
        {
            Path = file.Path;
            LastChange = file.LastChange;
        }

        [JsonConstructor]
        public BasicFileInfo(string resourcePath, DateTime lastChange)
        {
            Path = resourcePath;
            LastChange = lastChange;
        }

        public string Path { get; }
        public DateTime LastChange { get; }

        public bool Equals(BasicFileInfo other)
        {
            return Path == other.Path && LastChange.Equals(other.LastChange);
        }

        public override bool Equals(object obj)
        {
            return obj is BasicFileInfo other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Path.GetHashCode() * 397) ^ LastChange.GetHashCode();
            }
        }

        public static bool operator ==(BasicFileInfo left, BasicFileInfo right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(BasicFileInfo left, BasicFileInfo right)
        {
            return !left.Equals(right);
        }
    }
}