using System.Collections.Generic;
using UnityEngine;

namespace StlVault.AppModel.Messages
{
    internal class AddImportFolderMessage
    {
        public string FolderPath { get; set; }
        public bool ScanSubDirectories { get; set; }
        public Vector3 Rotation { get; set; }
        public bool RotateOnImport { get; set; }
        public Vector3 Scale { get; set; }
        public bool ScaleOnImport { get; set; }
        public IReadOnlyList<string> Tags { get; set; }
    }
}