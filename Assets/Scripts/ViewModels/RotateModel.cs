﻿using System;
using System.Linq;
 using System.Threading.Tasks;
 using System.Windows.Input;
using JetBrains.Annotations;
using StlVault.Services;
using StlVault.Util;
using StlVault.Util.Commands;
using UnityEngine;
 using static StlVault.ViewModels.RotationDirection;

 namespace StlVault.ViewModels
{
    internal class RotateModel
    {
        private readonly BindableProperty<bool> _running = new BindableProperty<bool>();
        private readonly DetailMenuModel _detailMenu;
        private readonly ILibrary _library;
        private SelectionMode Mode => _detailMenu.Mode;
        
        public ICommand<Rotation> RotateCommand { get; }
        public ICommand ResetRotationCommand { get; }
        
        public RotateModel([NotNull] DetailMenuModel detailMenu, [NotNull] ILibrary library)
        {
            _detailMenu = detailMenu ?? throw new ArgumentNullException(nameof(detailMenu));
            _library = library ?? throw new ArgumentNullException(nameof(library));
            
            RotateCommand = new DelegateCommand<Rotation>(CanRotate, Rotate)
                .UpdateOn(_detailMenu.AnythingSelected)
                .UpdateOn(_running);

            ResetRotationCommand = new DelegateCommand(() => CanRotate(null), ResetRotation)
                .UpdateOn(_detailMenu.AnythingSelected)
                .UpdateOn(_running);
        }

        private async void ResetRotation()
        {
            if (!CanRotate(null)) return;
            _running.Value = true;

            async Task ResetModel(ItemPreviewModel itemPreviewModel)
            {
                var rotation = _library.GetImportRotation(itemPreviewModel);
                await _library.RotateAsync(itemPreviewModel, rotation);
            }

            if (Mode == SelectionMode.Current)
            {
                await ResetModel(_detailMenu.Current.Value);
            }
            else if (Mode == SelectionMode.Selection)
            {
                foreach (var previewInfo in _detailMenu.Selection.ToList())
                {
                    await ResetModel(previewInfo);
                }
            }

            _running.Value = false;
        }

        private bool CanRotate(Rotation rot) => _detailMenu.AnythingSelected.Value && !_running;

        private async void Rotate(Rotation rotationDirection)
        {
            if (!CanRotate(rotationDirection)) return;
            _running.Value = true;
            
            switch (Mode)
            {
                case SelectionMode.Current:
                {
                    var previewInfo = _detailMenu.Current.Value;
                    var newRotation = GetRotation(previewInfo.GeometryInfo.Value.Rotation, rotationDirection);
                    await _library.RotateAsync(previewInfo, newRotation);
                    
                    break;
                }
                case SelectionMode.Selection:
                {
                    foreach (var previewInfo in _detailMenu.Selection.ToList())
                    {
                        var newRotation = GetRotation(previewInfo.GeometryInfo.Value.Rotation, rotationDirection);
                        await _library.RotateAsync(previewInfo, newRotation);
                    }

                    break;
                }
            }

            _running.Value = false;
        }

        private static Vector3 GetRotation(Vector3 current, Rotation rotation)
        {
            var cur = Quaternion.Euler(current);
            var amount = rotation.Kind == RotationKind.Big ? 90 : 10;
            
            switch (rotation.Direction)
            {
                case XClockwise:        return (Quaternion.AngleAxis(+amount, Vector3.right)   * cur).eulerAngles;
                case XCounterClockwise: return (Quaternion.AngleAxis(-amount, Vector3.right)   * cur).eulerAngles;
                case YClockwise:        return (Quaternion.AngleAxis(+amount, Vector3.up)      * cur).eulerAngles;
                case YCounterClockwise: return (Quaternion.AngleAxis(-amount, Vector3.up)      * cur).eulerAngles;
                case ZClockwise:        return (Quaternion.AngleAxis(+amount, Vector3.forward) * cur).eulerAngles;
                case ZCounterClockwise: return (Quaternion.AngleAxis(-amount, Vector3.forward) * cur).eulerAngles;
            }

            return current;
        }
    }

    internal enum RotationDirection
    {
        XClockwise,
        XCounterClockwise,
        YClockwise,
        YCounterClockwise,
        ZClockwise,
        ZCounterClockwise
    }

    internal class Rotation
    {
        public RotationDirection Direction { get; set; }
        public RotationKind Kind { get; set; }
    }

    internal enum RotationKind
    {
        Big,
        Small
    }
}