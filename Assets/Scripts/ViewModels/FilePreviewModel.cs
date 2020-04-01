using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using StlVault.Services;
using StlVault.Util;

namespace StlVault.ViewModels
{
    internal class FilePreviewModel : ModelBase
    {
        private readonly IPreviewImageStore _previewImageStore;
        private bool _inSelection;
        private bool _inFavorites;

        public string Name { get; set; }
        public string FileHash { get; set; }

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

        public Task<byte[]> LoadPreviewAsync()
        {
            return _previewImageStore.LoadPreviewAsync(FileHash);
        }

        public FilePreviewModel([NotNull] IPreviewImageStore previewImageStore)
        {
            _previewImageStore = previewImageStore ?? throw new ArgumentNullException(nameof(previewImageStore));
        }
    }
}