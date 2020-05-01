using System.Collections;
using DG.Tweening;
using StlVault.Util.Commands;
using StlVault.ViewModels;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

#pragma warning disable 0649

namespace StlVault.Views
{
    [RequireComponent(typeof(Button))]
    [RequireComponent(typeof(ContentSizeFitter))]
    [RequireComponent(typeof(HorizontalLayoutGroup))]
    internal class TagView : ViewBase<TagModel>
    {
        public bool PlayIntroAnimation { private get; set; } = true;

        [SerializeField] private Color _partialColor;
        [SerializeField] private Color _notColor;
        
        private const float TagFadeDuration = 0.2f;

        private HorizontalLayoutGroup _layoutGroup;
        private ContentSizeFitter _contentFitter;
        private Button _button;
        private TMP_Text _text;
        private RectTransform _parentTransform;
        private RectTransform _rect;
        private Tween _tween;
        private Image _image;

        private void Awake()
        {
            _button = GetComponent<Button>();
            _text = _button.GetComponentInChildren<TMP_Text>();
            _layoutGroup = GetComponent<HorizontalLayoutGroup>();
            _contentFitter = GetComponent<ContentSizeFitter>();
            _image = GetComponent<Image>();
        }

        protected override void OnViewModelBound()
        {
            _layoutGroup.enabled = true;
            _contentFitter.enabled = true;
            _text.text = ViewModel.Text;
            _button.onClick.AddListener(OnButtonClick);
            
            if (ViewModel.IsPartial) _image.color = _partialColor;
            if (ViewModel.Text.StartsWith("-")) _image.color = _notColor;
            
            StartCoroutine(DisableFitter());
        }

        private void Start()
        {
            _parentTransform = transform.parent.GetComponent<RectTransform>();

            _rect = GetComponent<RectTransform>();
            if (PlayIntroAnimation)
            {
                _rect.localScale = Vector3.zero;
                _tween = _rect.DOScale(Vector3.one, TagFadeDuration);
                StartCoroutine(TriggerParentResizeWhilePlaying());
            }
        }

        private void OnDestroy()
        {
            _tween?.Kill();
        }

        public void OnButtonClick()
        {
            _tween?.Kill();

            var sequence = DOTween.Sequence();
            var tweenSize = _rect
                .DOScale(Vector2.zero, TagFadeDuration)
                .SetEase(Ease.Linear);

            _tween = sequence.Append(tweenSize)
                .AppendCallback(ViewModel.RemoveCommand.Execute);

            StartCoroutine(TriggerParentResizeWhilePlaying());
        }

        private IEnumerator TriggerParentResizeWhilePlaying()
        {
            yield return new WaitForEndOfFrame();
            while (_tween.IsActive() && _tween.IsPlaying())
            {
                LayoutRebuilder.MarkLayoutForRebuild(_parentTransform);
                yield return new WaitForEndOfFrame();
            }
        }

        private IEnumerator DisableFitter()
        {
            yield return new WaitForEndOfFrame();

            _layoutGroup.enabled = false;
            _contentFitter.enabled = false;
        }
    }
}