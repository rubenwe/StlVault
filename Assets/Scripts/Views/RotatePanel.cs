using StlVault.Util.Commands;
using StlVault.Util.Unity;
using StlVault.ViewModels;
using UnityEngine;
using static StlVault.ViewModels.RotationDirection;

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
            _xClockwise.BindTo(ViewModel.RotateCommand,        () => new Rotation { Kind = GetKind(), Direction = XClockwise });
            _xCounterClockwise.BindTo(ViewModel.RotateCommand, () => new Rotation { Kind = GetKind(), Direction = XCounterClockwise});
            _zClockwise.BindTo(ViewModel.RotateCommand,        () => new Rotation { Kind = GetKind(), Direction = ZClockwise});
            _zCounterClockwise.BindTo(ViewModel.RotateCommand, () => new Rotation { Kind = GetKind(), Direction = ZCounterClockwise});
            _yClockwise.BindTo(ViewModel.RotateCommand,        () => new Rotation { Kind = GetKind(), Direction = YClockwise});
            _yCounterClockwise.BindTo(ViewModel.RotateCommand, () => new Rotation { Kind = GetKind(), Direction = YCounterClockwise});
        }

        private static RotationKind GetKind()
        {
            return Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt)
                ? RotationKind.Small
                : RotationKind.Big;
        }

        private void Update()
        {
            if (!gameObject.activeInHierarchy) return;
            
            var ctrl = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
            if (!ctrl) return;

            if (HandleResetRotation()) return;
            if (HandleRotation()) return;
        }

        private bool HandleRotation()
        {
            RotationDirection? rot = null;
            if (Input.GetKeyDown(KeyCode.LeftArrow)) rot = YClockwise;
            if (Input.GetKeyDown(KeyCode.RightArrow)) rot = YCounterClockwise;
            if (Input.GetKeyDown(KeyCode.UpArrow)) rot = XClockwise;
            if (Input.GetKeyDown(KeyCode.DownArrow)) rot = XCounterClockwise;

            if (!rot.HasValue) return true;

            var rotation = new Rotation {Kind = GetKind(), Direction = rot.Value};
            if (ViewModel.RotateCommand.CanExecute(rotation))
            {
                ViewModel.RotateCommand.Execute(rotation);
                return true;
            }

            return false;
        }

        private bool HandleResetRotation()
        {
            if (Input.GetKeyDown(KeyCode.Alpha0) || Input.GetKeyDown(KeyCode.Keypad0))
            {
                if (ViewModel.ResetRotationCommand.CanExecute())
                {
                    ViewModel.ResetRotationCommand.Execute();
                    return true;
                }
            }

            return false;
        }
    }
}