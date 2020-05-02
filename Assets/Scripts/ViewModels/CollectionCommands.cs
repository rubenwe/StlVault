using System.Windows.Input;

namespace StlVault.ViewModels
{
    internal class CollectionCommands
    {
        public ICommand Select { get; set; }
        public ICommand Add { get; set; }
        public ICommand Delete { get; set; }
    }
}