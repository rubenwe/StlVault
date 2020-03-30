using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using JetBrains.Annotations;
using StlVault.Config;
using UnityEngine;

namespace StlVault.Services
{
    [DebuggerDisplay("{" + nameof(ItemName) + "}")]
    internal class ItemPreviewMetadata : ITagged
    {
        public Vector3 Rotation { get; }
        public string StlFilePath { get; }
        public string ItemName { get; }
        public string PreviewImagePath { get; }
        public HashSet<string> Tags { get; }

        public ItemPreviewMetadata(
            [NotNull] string stlFilePath,
            [NotNull] IReadOnlyList<string> tags)
        {
            if (tags == null) throw new ArgumentNullException(nameof(tags));
            
            StlFilePath = stlFilePath ?? throw new ArgumentNullException(nameof(stlFilePath));
            ItemName = GetItemName(stlFilePath);
            PreviewImagePath = Path.ChangeExtension(stlFilePath, ".jpg");
            Tags = new HashSet<string>(tags);
        }

        private static string GetItemName(string stlFilePath)
        {
            return Path.GetFileNameWithoutExtension(stlFilePath)?
                .Replace("(repaired)", string.Empty)
                .Replace('_', ' ')
                .Trim();
        }
    }
}