using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using static StlVault.Util.Unity.Accordion.Orientation;

#pragma warning disable 0649

namespace StlVault.Util.Unity
{
    [RequireComponent(typeof(LayoutElement))]
    public class Accordion : MonoBehaviour
    {
        public enum Orientation
        {
            Horizontal,
            Vertical
        }

        [SerializeField] private Button _toggleButton;
        [SerializeField] private LayoutElement _panel;
        [SerializeField] private Orientation _orientation;
        [SerializeField] private bool _startExpanded = true;
        [SerializeField] private float _slideDuration = 1f;

        private bool _isExpanded;
        private bool _initialized;
        private float _initialSize;
        private Tween _tween;

        public bool IsExpanded
        {
            get => _initialized ? _isExpanded : _startExpanded;
            set
            {
                if (!_initialized)
                {
                    _startExpanded = value;
                    return;
                }

                if (_isExpanded != value) TogglePanel();
            }
        }

        void Start()
        {
            SetInitialSize();
            _isExpanded = _startExpanded;
            _toggleButton.onClick.AddListener(TogglePanel);

            if (!_startExpanded)
            {
                if (_orientation == Horizontal) _panel.flexibleWidth = 0;
                if (_orientation == Vertical) _panel.flexibleHeight = 0;
            }

            _initialized = true;
        }

        private void SetInitialSize()
        {
            _initialSize = _orientation == Horizontal
                ? _panel.flexibleWidth
                : _panel.flexibleHeight;
        }

        private void TogglePanel()
        {
            if (_tween != null && _tween.IsActive() && !_tween.IsComplete())
            {
                _tween.Kill();
            }

            var targetState = _isExpanded ? 0 : _initialSize;

            if (_orientation == Horizontal)
                _tween = DOTween.To(() => _panel.flexibleWidth, v => _panel.flexibleWidth = v, targetState,
                    _slideDuration);
            if (_orientation == Vertical)
                _tween = DOTween.To(() => _panel.flexibleHeight, v => _panel.flexibleHeight = v, targetState,
                    _slideDuration);

            _isExpanded = !_isExpanded;
        }
    }
}