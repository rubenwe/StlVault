using System;
using System.IO;
using DG.Tweening;
using StlVault.AppModel.ViewModels;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static StlVault.AppModel.ViewModels.FolderState;

#pragma warning disable 0649

namespace StlVault.Views
{
    internal class ImportFolderView : ViewBase<ImportFolderModel>
    {
        [SerializeField] private TMP_Text _text;
        [SerializeField] private Button _button;
        [SerializeField] private Image _icon;

        [Header("Icons")]
        [SerializeField] private Sprite _okSprite;
        [SerializeField] private Sprite _refreshingSprite;
        
        private Tween _tween;

        protected override void OnViewModelBound()
        {
            base.OnViewModelBound();

            _text.Bind(ViewModel.Path);
            ViewModel.Path.ValueChanged += OnPathOnValueChanged;
            void OnPathOnValueChanged(string s)
            {
                _stopTrimming = false;
                _lastTrimmingResult = null;
            }

            ViewModel.FolderState.ValueChanged += UpdateStateIcon;
            UpdateStateIcon(ViewModel.FolderState);
        }

        private void UpdateStateIcon(FolderState state)
        {
            _tween.Kill();
            
            switch (state)
            {
                case Ok:
                    _icon.sprite = _okSprite;
                    return;
                
                case Refreshing:
                    _icon.sprite = _refreshingSprite;
                    _tween = _icon.rectTransform
                        .DORotate(new Vector3(0, 360, 0), 1f)
                        .SetLoops(-1, LoopType.Incremental)
                        .OnKill(() => _icon.rectTransform.rotation = Quaternion.identity);
                    break;
            }
        }

        private string _lastTrimmingResult;
        private bool _stopTrimming;
        private void Update()
        {
            if (_stopTrimming) return;

            if (_text.isTextTruncated)
            {
                var displayed = _text.textInfo.characterCount;
                var maximum = _text.text.Length;

                var lastDir = _text.text.LastIndexOf(Path.DirectorySeparatorChar, displayed / 2) + 1;
                var startString = _text.text.Substring(0, Math.Max(0, lastDir));
                var endString = _text.text.Substring(Math.Max(0, maximum - displayed / 2));
                var combined = startString + "..." + endString;
                _text.text = combined;

                if (_lastTrimmingResult == combined) _stopTrimming = true;
                _lastTrimmingResult = combined;
            }
            else _stopTrimming = true;
        }
    }
}