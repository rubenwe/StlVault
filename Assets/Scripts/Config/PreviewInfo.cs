using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using JetBrains.Annotations;
using Newtonsoft.Json;
using StlVault.Services;
using UnityEngine.Scripting;

namespace StlVault.Config
{
    [DebuggerDisplay("{" + nameof(ItemName) + "}")]
    internal class PreviewInfo
    {
        public string ItemName { get; set; }
        public string FileHash { get; set; }
        public int Resolution { get; set; }
        public HashSet<string> Tags { get; set; }
        public List<ImportedFileInfo> Sources { get; set; }
        
        public int VertexCount { get; set; }
        public float Volume { get; set; }
        public ConfigVector3 Size { get; set; }
        
        [DefaultValue(null)]
        public ConfigVector3? Rotation { get; set; }
        
        [DefaultValue(null)]
        public ConfigVector3? Scale { get; set; }
    }
}