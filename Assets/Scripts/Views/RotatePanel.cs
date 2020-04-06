using System.Collections.Generic;
using System.Linq;
using StlVault.Util.Unity;
using StlVault.ViewModels;

namespace StlVault.Views
{
    internal class RotatePanel : ViewBase<RotateModel>
    {
        private List<SimpleButton> _buttons;

        private void Awake()
        {
            _buttons = GetComponentsInChildren<SimpleButton>().ToList();
        }

        protected override void OnViewModelBound()
        {
            base.OnViewModelBound();

            _buttons[0].Bind(ViewModel.RotateCommand, Rotation.XClockwise);
            _buttons[1].Bind(ViewModel.RotateCommand, Rotation.XCounterClockwise);
            _buttons[2].Bind(ViewModel.RotateCommand, Rotation.ZClockwise);
            _buttons[3].Bind(ViewModel.RotateCommand, Rotation.ZCounterClockwise);
            _buttons[4].Bind(ViewModel.RotateCommand, Rotation.YClockwise);
            _buttons[5].Bind(ViewModel.RotateCommand, Rotation.YCounterClockwise);
        }
    }
}