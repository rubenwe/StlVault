using System;
using System.Linq;
using System.Windows.Input;
using JetBrains.Annotations;
using StlVault.Messages;
using StlVault.Services;
using StlVault.Util.Commands;
using StlVault.Util.Messaging;

namespace StlVault.ViewModels
{
    internal class ViewPanelModel
    {
        private readonly DetailMenuModel _detailMenuModel;
        private readonly ILibrary _library;
        private readonly IMessageRelay _relay;
        public ICommand OpenIn3DViewCommand { get; }
        public ICommand ShowInExplorerCommand { get; }

        public ViewPanelModel(
            [NotNull] DetailMenuModel detailMenuModel,
            [NotNull] ILibrary library,
            [NotNull] IMessageRelay relay)
        {
            _detailMenuModel = detailMenuModel ?? throw new ArgumentNullException(nameof(detailMenuModel));
            _library = library ?? throw new ArgumentNullException(nameof(library));
            _relay = relay ?? throw new ArgumentNullException(nameof(relay));

            OpenIn3DViewCommand = new DelegateCommand(CanOpenIn3DView, OpenIn3DView)
                .UpdateOn(_detailMenuModel.AnythingSelected);

            ShowInExplorerCommand = new DelegateCommand(CanOpenInExplorer, OpenInExplorer)
                .UpdateOn(_detailMenuModel.Mode)
                .UpdateOn(_detailMenuModel.Current);
        }

        private bool CanOpenIn3DView() => _detailMenuModel.AnythingSelected.Value;

        private bool CanOpenInExplorer() =>
            _detailMenuModel.Mode == SelectionMode.Current
            && _detailMenuModel.Current != null
            && _library.TryGetLocalPath(_detailMenuModel.Current.Value, out _);

        private void OpenIn3DView()
        {
            _relay.Send(this, new RequestShowDialogMessage.EditScreen
            {
                SelectedItems = _detailMenuModel.Mode == SelectionMode.Current
                    ? new[] {_detailMenuModel.Current.Value}
                    : _detailMenuModel.Selection.ToArray()
            });
        }

        private void OpenInExplorer()
        {
            if (_library.TryGetLocalPath(_detailMenuModel.Current.Value, out var path))
            {
                NativeMethods.BrowseTo(path);
            }
        }
    }
}