using System.Threading.Tasks;
using DG.Tweening;
using StlVault.Util.Commands;
using StlVault.Util.Unity;
using StlVault.ViewModels;
using TMPro;
using UnityEngine;

#pragma warning disable 0649

namespace StlVault.Views
{
    [RequireComponent(typeof(SimpleButton))]
    internal class AppVersionButton : ViewBase<AppVersionDisplayModel>
    {
        [SerializeField] private TMP_Text _text;
        [SerializeField] private Color _updateAvailableColor;
        private SimpleButton _button;

        private void Awake()
        {
            _button = GetComponent<SimpleButton>();
        }

        protected override void OnViewModelBound()
        {
            _text.BindTo(ViewModel.CurrentVersion);
            _button.BindTo(ViewModel.OpenUpdateDialogCommand);
            ViewModel.OpenUpdateDialogCommand.CanExecuteChanged += (s, e) => CanExecuteChanged();
            CanExecuteChanged();
            _button.Clicked += ButtonOnClicked;
        }

        private void ButtonOnClicked()
        {
            _text.DOKill(true);
            _text.DOColor(_updateAvailableColor, 0.1f);
        }

        private async void CanExecuteChanged()
        {
            await Task.Delay(500);
            if (ViewModel.OpenUpdateDialogCommand.CanExecute())
            {
                _text.DOColor(_updateAvailableColor, 1f).SetLoops(-1, LoopType.Yoyo);
            }
        }
    }
}