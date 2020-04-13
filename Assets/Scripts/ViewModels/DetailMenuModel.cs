using System;
using System.Linq;
using System.Windows.Input;
using JetBrains.Annotations;
using StlVault.Messages;
using StlVault.Services;
using StlVault.Util;
using StlVault.Util.Collections;
using StlVault.Util.Commands;
using StlVault.Util.Messaging;
using UnityEditor;

namespace StlVault.ViewModels
{
    internal class DetailMenuModel :
        IMessageReceiver<SelectionChangedMessage>,
        IMessageReceiver<MassSelectionStartingMessage>,
        IMessageReceiver<MassSelectionFinishedMessage>
    {
        public BindableProperty<SelectionMode> Mode { get; } = new BindableProperty<SelectionMode>();

        public BindableProperty<ItemPreviewModel> Current { get; } = new BindableProperty<ItemPreviewModel>();
        public ObservableList<ItemPreviewModel> Selection { get; } = new ObservableList<ItemPreviewModel>();

        public ICommand SwitchToCurrentModeCommand { get; }
        public ICommand SwitchToSelectionModeCommand { get; }

        public StatsModel StatsModel { get; }
        public TagEditorModel TagEditorModel { get; }
        public RotateModel RotateModel { get; }
        public ViewPanelModel ViewPanelModel { get; }
        
        public IBindableProperty<bool> AnythingSelected { get; }
        

        private bool IsSomethingSelected() => Mode == SelectionMode.Current
            ? Current.Value != null
            : Selection.Any();

        public DetailMenuModel([NotNull] ILibrary library, [NotNull] IMessageRelay relay)
        {
            if (library == null) throw new ArgumentNullException(nameof(library));
            if (relay == null) throw new ArgumentNullException(nameof(relay));

            SwitchToCurrentModeCommand = new DelegateCommand(
                () => Mode != SelectionMode.Current,
                () => Mode.Value = SelectionMode.Current);

            SwitchToSelectionModeCommand = new DelegateCommand(
                () => Mode != SelectionMode.Selection,
                () => Mode.Value = SelectionMode.Selection);

            Mode.ValueChanged += ModeOnValueChanged;
            
            AnythingSelected = new DelegateProperty<bool>(IsSomethingSelected)
                .UpdateOn(Mode)
                .UpdateOn(Current)
                .UpdateOn(Selection);
            
            StatsModel = new StatsModel(this);
            TagEditorModel = new TagEditorModel(this, library);
            RotateModel = new RotateModel(this, library);
            ViewPanelModel = new ViewPanelModel(this, library, relay);
        }

        private void ModeOnValueChanged(SelectionMode obj)
        {
            SwitchToCurrentModeCommand.OnCanExecuteChanged();
            SwitchToSelectionModeCommand.OnCanExecuteChanged();
        }

        public void Receive(SelectionChangedMessage message)
        {
            var sender = message.Sender;
            if (sender.Selected)
            {
                if (Selection.Count == 1) Mode.Value = SelectionMode.Selection;
                Current.Value = sender;
                Selection.Add(sender);
            }
            else
            {
                Current.Value = null;
                Selection.Remove(sender);
                if (Selection.Count == 1) Mode.Value = SelectionMode.Current;
            }
        }

        private IDisposable _massUpdate;
        public void Receive(MassSelectionStartingMessage message)
        {
            _massUpdate?.Dispose();
            _massUpdate = Selection.EnterMassUpdate();
            Current.SuppressUpdates();
        }

        public void Receive(MassSelectionFinishedMessage message)
        {
            _massUpdate?.Dispose();
            _massUpdate = null;
            Current.ResumeUpdates();
        }
    }
}