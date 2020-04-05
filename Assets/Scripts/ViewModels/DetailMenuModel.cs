using System;
using System.Windows.Input;
using JetBrains.Annotations;
using StlVault.Config;
using StlVault.Services;
using StlVault.Util;
using StlVault.Util.Collections;
using StlVault.Util.Commands;

namespace StlVault.ViewModels
{
    internal class DetailMenuModel : ModelBase
    {
        private readonly ISelectionTracker _tracker;
        public BindableProperty<SelectionMode> Mode { get; } = new BindableProperty<SelectionMode>();
        public BindableProperty<PreviewInfo> Current => _tracker.CurrentSelected;
        public IReadOnlyObservableCollection<PreviewInfo> Selection => _tracker.Selection;
        
        public ICommand SwitchToCurrentModeCommand { get; }
        public ICommand SwitchToSelectionModeCommand { get; }
        
        public StatsModel StatsModel { get; }
        public TagEditorModel TagEditorModel { get; }

        public DetailMenuModel([NotNull] ISelectionTracker tracker, [NotNull] ILibrary library)
        {
            if (library == null) throw new ArgumentNullException(nameof(library));
            _tracker = tracker ?? throw new ArgumentNullException(nameof(tracker));
            
            SwitchToCurrentModeCommand = new DelegateCommand(
                () => Mode != SelectionMode.Current,
                () => Mode.Value = SelectionMode.Current);
            
            SwitchToSelectionModeCommand = new DelegateCommand(
                () => Mode != SelectionMode.Selection,
                () => Mode.Value = SelectionMode.Selection);
            
            Mode.ValueChanged += ModeOnValueChanged;

            StatsModel = new StatsModel(this);
            TagEditorModel = new TagEditorModel(this, library);
        }

        private void ModeOnValueChanged(SelectionMode obj)
        {
            SwitchToCurrentModeCommand.OnCanExecuteChanged();
            SwitchToSelectionModeCommand.OnCanExecuteChanged();
        }
    }
}