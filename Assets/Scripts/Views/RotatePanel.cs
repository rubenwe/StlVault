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
            _xClockwise.BindTo(ViewModel.RotateCommand, Rotation.XClockwise);
            _xCounterClockwise.BindTo(ViewModel.RotateCommand, Rotation.XCounterClockwise);
            _zClockwise.BindTo(ViewModel.RotateCommand, Rotation.ZClockwise);
            _zCounterClockwise.BindTo(ViewModel.RotateCommand, Rotation.ZCounterClockwise);
            _yClockwise.BindTo(ViewModel.RotateCommand, Rotation.YClockwise);
            _yCounterClockwise.BindTo(ViewModel.RotateCommand, Rotation.YCounterClockwise);
        }
    }
}