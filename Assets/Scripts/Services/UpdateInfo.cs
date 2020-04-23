using System.Diagnostics.CodeAnalysis;
using UnityEngine.Scripting;

namespace StlVault.Services
{
    [Preserve]
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    internal class UpdateInfo
    {
        [Preserve] public string Version { [Preserve] get; [Preserve] set; }
        [Preserve] public string UpdateUrl { [Preserve] get; [Preserve] set; }
        [Preserve] public string Changes { [Preserve] get; [Preserve] set; }
    }
}