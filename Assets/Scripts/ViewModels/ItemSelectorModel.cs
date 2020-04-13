using StlVault.Util.Collections;

namespace StlVault.ViewModels
{
    internal class ItemSelectorModel 
    {
        public ObservableList<ItemPreviewModel> Models { get; } = new ObservableList<ItemPreviewModel>();
        public ObservableList<ItemPreviewModel> Selected { get; } = new ObservableList<ItemPreviewModel>();
    }
}