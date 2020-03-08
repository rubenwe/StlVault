using System.Collections.Generic;

namespace StlVault.Config
{
    public class CollectionConfig : List<CollectionConfig>
    {
        public string Name { get; set; }
    }
}