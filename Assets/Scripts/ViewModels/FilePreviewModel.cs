using System;
using System.Threading.Tasks;
using System.Windows.Input;
using JetBrains.Annotations;
using StlVault.Services;
using StlVault.Util;
using StlVault.Util.Commands;

namespace StlVault.ViewModels
{
    internal class FilePreviewModel : ModelBase
    {
        private readonly IPreviewImageStore _previewImageStore;
        public string Name { get; set; }
        public string FileHash { get; set; }

        public BindableProperty<bool> Selected { get; } = new BindableProperty<bool>();
        public ICommand SelectCommand { get; }

        public Task<byte[]> LoadPreviewAsync()
        {
            return _previewImageStore.LoadPreviewAsync(FileHash);
        }

        public FilePreviewModel([NotNull] IPreviewImageStore previewImageStore, [NotNull] Action onSelected)
        {
            _previewImageStore = previewImageStore ?? throw new ArgumentNullException(nameof(previewImageStore));
            SelectCommand = new DelegateCommand(() =>
            {
                Selected.Value = !Selected;
                onSelected();
            });
        }
    }
}