using UnityEngine;
using UnityEngine.UI;

#pragma warning disable 0649

namespace StlVault.Util.Unity
{
    [RequireComponent(typeof(RectTransform))]
    public class AdaptiveTopPadding : MonoBehaviour
    {
        [SerializeField] private int _basePadding;
        [SerializeField] private RectTransform _paddingDriver;
        
        private LayoutGroup _layoutGroup;
        private RectTransform _rectTransform;

        private void Start()
        {
            _layoutGroup = GetComponent<LayoutGroup>();
            _rectTransform = GetComponent<RectTransform>();
        }

        public void Update()
        {
            var newValue = _basePadding + (int) _paddingDriver.rect.height;
            if (_layoutGroup.padding.top == newValue) return;
            _layoutGroup.padding.top = newValue;
            
            LayoutRebuilder.MarkLayoutForRebuild(_rectTransform);
        }
    }
}