using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using JetBrains.Annotations;
using StlVault.Config;
using UnityEngine;

namespace StlVault.Services
{
    [DebuggerDisplay("{" + nameof(FileName) + "}")]
    internal class PreviewInfo : ITagged
    {
        public string FileName { get; }
        
        public string FileHash { get;  }
        public HashSet<string> Tags { get; }

        public PreviewInfo(
            [NotNull] string fileName,
            [NotNull] string fileHash,
            [NotNull] IReadOnlyList<string> tags)
        {
            FileName = fileName ?? throw new ArgumentNullException(nameof(fileName));
            FileHash = fileHash ?? throw new ArgumentNullException(nameof(fileHash));
            if (tags is null) throw new ArgumentNullException(nameof(tags));
            Tags = new HashSet<string>(tags);
        }
    }
}