using System;
using System.Windows.Input;
using JetBrains.Annotations;
using StlVault.Services;
using StlVault.Util;
using StlVault.Util.Commands;

namespace StlVault.ViewModels
{
    internal class FileSourceModel 
    {
        public BindableProperty<string> Path { get; } = new BindableProperty<string>();

        [NotNull] public BindableProperty<FileSourceState> State => FileSource.State;
        [NotNull] public IFileSource FileSource { get; }

        public ICommand SelectCommand { get; }
        public ICommand EditCommand { get; }
        public ICommand DeleteCommand { get; }

        public FileSourceModel(
            [NotNull] IFileSource source,
            [NotNull] Action<FileSourceModel> onSelect,
            [NotNull] Action<FileSourceModel> onEdit,
            [NotNull] Action<FileSourceModel> onDelete)
        {
            if (onSelect == null) throw new ArgumentNullException(nameof(onSelect));
            if (onEdit == null) throw new ArgumentNullException(nameof(onEdit));
            if (onDelete == null) throw new ArgumentNullException(nameof(onDelete));
            FileSource = source ?? throw new ArgumentNullException(nameof(source));

            SelectCommand = new DelegateCommand(() => onSelect(this));
            EditCommand = new DelegateCommand(() => onEdit(this));
            DeleteCommand = new DelegateCommand(() => onDelete(this));

            Path.Value = FileSource.DisplayName;
        }
    }
}