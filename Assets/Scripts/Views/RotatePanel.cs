using StlVault.Util.Unity;
using StlVault.ViewModels;
using UnityEngine;

#pragma warning disable 0649

namespace StlVault.Views
{
    internal class RotatePanel : ViewBase<RotateModel>
    {
        [SerializeField] private SimpleButton _xClockwise;
        [SerializeField] private SimpleButton _xCounterClockwise;
        [SerializeField] private SimpleButton _yClockwise;
        [SerializeField] private SimpleButton _yCounterClockwise;
        [SerializeField] private SimpleButton _zClockwise;
        [SerializeField] private SimpleButton _zCounterClockwise;

        protected override void OnViewModelBound()
        {
            base.OnViewModelBound();

            _xClockwise.Bind(ViewModel.RotateCommand, Rotation.XClockwise);
            _xCounterClockwise.Bind(ViewModel.RotateCommand, Rotation.XCounterClockwise);
            _zClockwise.Bind(ViewModel.RotateCommand, Rotation.ZClockwise);
            _zCounterClockwise.Bind(ViewModel.RotateCommand, Rotation.ZCounterClockwise);
            _yClockwise.Bind(ViewModel.RotateCommand, Rotation.YClockwise);
            _yCounterClockwise.Bind(ViewModel.RotateCommand, Rotation.YCounterClockwise);
        }
    }
}