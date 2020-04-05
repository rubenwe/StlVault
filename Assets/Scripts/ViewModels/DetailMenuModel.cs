using System.Windows.Input;
using StlVault.Config;
using StlVault.Services;
using StlVault.Util;
using StlVault.Util.Collections;
using StlVault.Util.Commands;

namespace StlVault.ViewModels
{
    internal class DetailMenuModel : ModelBase
    {
        private readonly ILibrary _library;
        public BindableProperty<SelectionMode> Mode { get; } = new BindableProperty<SelectionMode>();
        public BindableProperty<PreviewInfo> Current => _library.CurrentSelected;
        public IReadOnlyObservableList<PreviewInfo> Selection => _library.Selection;
        
        public ICommand SwitchToCurrentModeCommand { get; }
        public ICommand SwitchToSelectionModeCommand { get; }
        
        public StatsModel StatsModel { get; }
        public TagEditorModel TagEditorModel { get; }

        public DetailMenuModel(ILibrary library)
        {
            _library = library;
            
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