using System;
using System.Collections.Generic;
using System.IO;
using JetBrains.Annotations;
using StlVault.Config;
using UnityEngine;

namespace StlVault.AppModel
{
    internal class ItemPreviewMetadata : ITagged
    {
        private readonly ImportFolderConfig _folderConfig;
        
        public string ImportFolderPath => _folderConfig.FullPath;
        public Vector3? Scale => _folderConfig.Scale;
        public Vector3? Rotation => _folderConfig.Rotation;
        
        public string StlFilePath { get; }
        public string ItemName { get;  }
        public string PreviewImagePath { get; }
        public HashSet<string> Tags { get; }

        public ItemPreviewMetadata(
            [NotNull] string stlFilePath,
            [NotNull] IReadOnlyList<string> tags,
            [NotNull] ImportFolderConfig folderConfig)
        {
            if (tags == null) throw new ArgumentNullException(nameof(tags));
            _folderConfig = folderConfig ?? throw new ArgumentNullException(nameof(folderConfig));

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