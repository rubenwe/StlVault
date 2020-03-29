using System;
using System.Windows.Input;
using JetBrains.Annotations;
using StlVault.Services;
using StlVault.Util;
using StlVault.Util.Commands;

namespace StlVault.ViewModels
{
    internal class ImportFolderModel : ModelBase
    {
        public BindableProperty<string> Path { get; } = new BindableProperty<string>();

        [NotNull] public BindableProperty<FolderState> FolderState => ImportFolder.FolderState;
        [NotNull] public IImportFolder ImportFolder { get; }

        public ICommand SelectCommand { get; }
        public ICommand EditCommand { get; }
        public ICommand DeleteCommand { get; }

        public ImportFolderModel(
            [NotNull] IImportFolder folder,
            [NotNull] Action<ImportFolderModel> onSelect,
            [NotNull] Action<ImportFolderModel> onEdit,
            [NotNull] Action<ImportFolderModel> onDelete)
        {
            if (onSelect == null) throw new ArgumentNullException(nameof(onSelect));
            if (onEdit == null) throw new ArgumentNullException(nameof(onEdit));
            if (onDelete == null) throw new ArgumentNullException(nameof(onDelete));
            ImportFolder = folder ?? throw new ArgumentNullException(nameof(folder));

            SelectCommand = new DelegateCommand(() => onSelect(this));
            EditCommand = new DelegateCommand(() => onEdit(this));
            DeleteCommand = new DelegateCommand(() => onDelete(this));

            Path.Value = ImportFolder.Config.FullPath;
        }
    }
}