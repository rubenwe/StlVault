using StlVault.Config;
using StlVault.Util;

namespace StlVault.ViewModels
{
    internal class CollectionModel : ModelBase
    {
        public BindableProperty<string> Name { get; } = new BindableProperty<string>();

        public CollectionModel(CollectionConfig config)
        {
            Name.Value = config.Name;
        }
    }
}