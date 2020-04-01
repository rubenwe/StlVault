using System.Collections.Generic;

namespace StlVault.Config
{
    internal class KnownFiles : Dictionary<string, Dictionary<string, ImportedFileInfo>>
    {
        public new Dictionary<string, ImportedFileInfo> this[string sourceId]
        {
            get
            {
                if (!TryGetValue(sourceId, out var list))
                {
                    list = this[sourceId] = new Dictionary<string, ImportedFileInfo>();
                }

                return list;
            }
            private set => base[sourceId] = value;
        }
    }
}