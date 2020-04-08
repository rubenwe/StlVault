﻿using System;
using System.Linq;
using System.Windows.Input;
using JetBrains.Annotations;
using StlVault.Services;
using StlVault.Util;
using StlVault.Util.Commands;
using UnityEngine;

namespace StlVault.ViewModels
{
    internal class RotateModel
    {
        private readonly BindableProperty<bool> _running = new BindableProperty<bool>();
        private readonly DetailMenuModel _detailMenu;
        private readonly ILibrary _library;
        private SelectionMode Mode => _detailMenu.Mode;
        
        public ICommand RotateCommand { get; }
        
        public RotateModel([NotNull] DetailMenuModel detailMenu, [NotNull] ILibrary library)
        {
            _detailMenu = detailMenu ?? throw new ArgumentNullException(nameof(detailMenu));
            _library = library ?? throw new ArgumentNullException(nameof(library));
            
            RotateCommand = new DelegateCommand<Rotation>(CanRotate, Rotate)
                .UpdateOn(_detailMenu.AnythingSelected)
                .UpdateOn(_running);
        }

        private bool CanRotate(Rotation rot) => _detailMenu.AnythingSelected.Value && !_running;

        private async void Rotate(Rotation rotation)
        {
            if (!CanRotate(rotation)) return;
            _running.Value = true;
            
            switch (Mode)
            {
                case SelectionMode.Current:
                {
                    var previewInfo = _detailMenu.Current.Value;
                    var newRotation = GetRotation(previewInfo.GeometryInfo.Value.Rotation, rotation);
                    await _library.RotateAsync(previewInfo, newRotation);
                    
                    break;
                }
                case SelectionMode.Selection:
                {
                    foreach (var previewInfo in _detailMenu.Selection.ToList())
                    {
                        var newRotation = GetRotation(previewInfo.GeometryInfo.Value.Rotation, rotation);
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
            switch (rotation)
            {
                case Rotation.XClockwise:        return (Quaternion.AngleAxis(+90, Vector3.right)   * cur).eulerAngles;
                case Rotation.XCounterClockwise: return (Quaternion.AngleAxis(-90, Vector3.right)   * cur).eulerAngles;
                case Rotation.YClockwise:        return (Quaternion.AngleAxis(+90, Vector3.up)      * cur).eulerAngles;
                case Rotation.YCounterClockwise: return (Quaternion.AngleAxis(-90, Vector3.up)      * cur).eulerAngles;
                case Rotation.ZClockwise:        return (Quaternion.AngleAxis(+90, Vector3.forward) * cur).eulerAngles;
                case Rotation.ZCounterClockwise: return (Quaternion.AngleAxis(-90, Vector3.forward) * cur).eulerAngles;
            }

            return current;
        }
    }

    internal enum Rotation
    {
        XClockwise,
        XCounterClockwise,
        YClockwise,
        YCounterClockwise,
        ZClockwise,
        ZCounterClockwise
    }
}