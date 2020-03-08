using StlVault.Util;

namespace StlVault.AppModel.ViewModels
{
    public class ItemModel : ModelBase
    {
        private bool _inSelection;
        private bool _inFavorites;
        private string _name;
        private string _previewImagePath;

        public string Name
        {
            get => _name;
            set => SetValueAndNotify(ref _name, value);
        }

        public bool InFavorites
        {
            get => _inFavorites;
            set => SetValueAndNotify(ref _inFavorites, value);
        }

        public bool InSelection
        {
            get => _inSelection;
            set => SetValueAndNotify(ref _inSelection, value);
        }

        public string PreviewImagePath
        {
            get => _previewImagePath;
            set => SetValueAndNotify(ref _previewImagePath, value);
        }
    }
}
    