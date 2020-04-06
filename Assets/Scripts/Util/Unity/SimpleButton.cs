using System;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

#pragma warning disable 0649

namespace StlVault.Util.Unity
{
    internal class SimpleButton : MonoBehaviour, 
        IPointerEnterHandler, IPointerExitHandler, 
        IPointerDownHandler, IPointerUpHandler, IPointerClickHandler
    {
        [SerializeField] private Image _image;
        [SerializeField] private Color _hoverColor;
        [SerializeField] private Color _pressedColor;
        [SerializeField] private Color _disabledColor;
        [SerializeField] private bool _enabled = true;
        private Color _normalColor;

        public event Action Clicked;
        public BindableProperty<bool> Enabled { get; } = new BindableProperty<bool>();
        private readonly BindableProperty<bool> _pointerInside = new BindableProperty<bool>();
        private readonly BindableProperty<bool> _pointerDown = new BindableProperty<bool>();
        private readonly IBindableProperty<Color> _currentColor;
        
        public SimpleButton()
        {
            _currentColor = new DelegateProperty<Color>(DetermineColor)
                .UpdateOn(_pointerInside)
                .UpdateOn(_pointerDown)
                .UpdateOn(Enabled);
        }

        private Color DetermineColor()
        {
            if (!Enabled) return _normalColor;
            if (_pointerDown) return _pressedColor;
            if (_pointerInside) return _hoverColor;
            return _normalColor;
        }

        private void Awake()
        {
            if (_image == null) _image = GetComponentInChildren<Image>();
            _normalColor = _image.color;
            _currentColor.ValueChanged += color => _image.color = color;
            
            var childImage = GetComponentsInChildren<Image>()
                .First(c => c.gameObject != gameObject);

            var childColor = childImage.color;

            Enabled.ValueChanged += on => childImage.color = on ? childColor : _disabledColor;
            Enabled.Value = _enabled;
        }

        public void OnPointerEnter(PointerEventData eventData) => _pointerInside.Value = true;
        public void OnPointerExit(PointerEventData eventData) => _pointerInside.Value = false;
        public void OnPointerUp(PointerEventData eventData) => _pointerDown.Value = false;
        public void OnPointerDown(PointerEventData eventData) => _pointerDown.Value = true;

        public void OnPointerClick(PointerEventData eventData)
        {
            if(Enabled) Clicked?.Invoke();
        }
    }
}