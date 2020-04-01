using System;
using System.IO;
using System.Windows.Forms;
using DG.Tweening;
using StlVault.Services;
using StlVault.ViewModels;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static StlVault.Services.FileSourceState;
using Button = UnityEngine.UI.Button;

#pragma warning disable 0649

namespace StlVault.Views
{
    internal class ImportFolderView : ViewBase<FileSourceModel>
    {
        [SerializeField] private TMP_Text _text;
        [SerializeField] private Button _selectButton;
        [SerializeField] private Button _deleteButton;
        [SerializeField] private Image _icon;
        [SerializeField] private Color _highlightColor = new Color(1f, 1f, 0.6f, 1f);

        [Header("Icons")] 
        [SerializeField] private Sprite _okSprite;
        [SerializeField] private Sprite _refreshingSprite;

        protected override void OnViewModelBound()
        {
            base.OnViewModelBound();

            _selectButton.Bind(ViewModel.SelectCommand);
            _deleteButton.Bind(ViewModel.DeleteCommand);

            ViewModel.State.OnMainThread().ValueChanged += UpdateStateIcon;
            UpdateStateIcon(ViewModel.State);
            
            _text.Bind(ViewModel.Path);
            ViewModel.Path.ValueChanged += OnPathOnValueChanged;

            void OnPathOnValueChanged(string s)
            {
                _stopTrimming = false;
                _lastTrimmingResult = null;
            }
        }

        private void UpdateStateIcon(FileSourceState state)
        {
            if (_icon == null) return;
            
            // Kill all animations
            _icon.DOKill();

            // Skip strong animations on startup
            if (Time.time > 0.2f)
            {
                _icon.rectTransform.DOPunchScale(-0.7f * Vector3.one, 0.3f, 10, 0.5f)
                    .OnKill(() => _icon.rectTransform.localScale = Vector3.one);
            }

            if (state == Ok)
            {
                _icon.sprite = _okSprite;
            }
            else if (state == Refreshing)
            {
                _icon.sprite = _refreshingSprite;
                
                // Cycle highlight/default color and reset when done
                var color = _icon.color;
                _icon.DOColor(_highlightColor, duration: 0.7f)
                    .SetLoops(-1, LoopType.Yoyo)
                    .OnKill(() => _icon.color = color);
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