using System;
using System.Collections.Generic;
using System.Diagnostics;
using JetBrains.Annotations;
using Newtonsoft.Json;
using StlVault.Services;
using UnityEngine.Scripting;

namespace StlVault.Config
{
    [DebuggerDisplay("{" + nameof(ItemName) + "}")]
    internal class PreviewInfo : ITagged
    {
        [JsonIgnore]
        public string FileHash { get; set; }

        public string ItemName { get; }
        public HashSet<string> Tags { get; }
        
        public PreviewInfo(
            [NotNull] string itemName,
            [NotNull] string fileHash,
            [NotNull] HashSet<string> tags)
        {
            ItemName = itemName ?? throw new ArgumentNullException(nameof(itemName));
            FileHash = fileHash ?? throw new ArgumentNullException(nameof(fileHash));
            Tags = tags ?? throw new ArgumentNullException(nameof(tags));
        }
        
        [JsonConstructor, Preserve]
        public PreviewInfo(
            [NotNull] string itemName,
            [NotNull] HashSet<string> tags)
        {
            ItemName = itemName ?? throw new ArgumentNullException(nameof(itemName));
            Tags = tags ?? throw new ArgumentNullException(nameof(tags));
        }
    }
}