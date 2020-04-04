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
        public string ItemName { get; set; }
        public HashSet<string> Tags { get; set; }
        
        public float Volume { get; set; }
        public GeometryInfo GeometryInfo { get; set; }
    }
}