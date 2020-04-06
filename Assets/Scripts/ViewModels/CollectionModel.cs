using System.ComponentModel;
using StlVault.Config;
using StlVault.Util;

namespace StlVault.ViewModels
{
    internal class CollectionModel
    {
        public BindableProperty<string> Name { get; } = new BindableProperty<string>();

        public CollectionModel(CollectionConfig config)
        {
            Name.Value = config.Name;
        }
    }
}