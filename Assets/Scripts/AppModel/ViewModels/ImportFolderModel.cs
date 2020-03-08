using System;
using System.Windows.Input;
using JetBrains.Annotations;
using StlVault.Config;
using StlVault.Util;
using StlVault.Util.Commands;

namespace StlVault.AppModel.ViewModels
{
    internal class ImportFolderModel : ModelBase
    {
        public BindableProperty<string> Path { get; } = new BindableProperty<string>();
        public BindableProperty<FolderState> FolderState { get; } = new BindableProperty<FolderState>();
        public ImportFolderConfig Config { get; }

        public ICommand SelectCommand { get; }
        public ICommand EditCommand { get; }
        public ICommand DeleteCommand { get; }
        
        public ImportFolderModel(
            [NotNull] ImportFolderConfig config,
            [NotNull] Action<ImportFolderModel> onSelect,
            [NotNull] Action<ImportFolderModel> onEdit,
            [NotNull] Action<ImportFolderModel> onDelete)
        {
            if (onSelect == null) throw new ArgumentNullException(nameof(onSelect));
            if (onEdit == null) throw new ArgumentNullException(nameof(onEdit));
            if (onDelete == null) throw new ArgumentNullException(nameof(onDelete));
            Config = config ?? throw new ArgumentNullException(nameof(config));
            
            SelectCommand = new DelegateCommand(() => onSelect(this));
            EditCommand = new DelegateCommand(() => onEdit(this));
            DeleteCommand = new DelegateCommand(() => onDelete(this));

            Path.Value = config.FullPath;
        }
    }
}