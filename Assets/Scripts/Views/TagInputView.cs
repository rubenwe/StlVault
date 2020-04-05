using System.Collections.Specialized;
using System.Linq;
using DG.Tweening;
using StlVault.Util.Collections;
using StlVault.Util.Commands;
using StlVault.Util.Unity;
using StlVault.ViewModels;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static UnityEngine.KeyCode;

#pragma warning disable 0649

namespace StlVault.Views
{
    internal class TagInputView : ContainerView<TagInputModelBase, TagView, TagModel>
    {
        private const float PanelFadeDuration = 0.2f;

        [SerializeField] private SuggestionView _suggestionPrefab;
        [SerializeField] private TMP_InputField _searchInputField;
        [SerializeField] private Transform _autocompleteContainer;
        [SerializeField] private Transform _autocompleteParent;
        [SerializeField] private bool _playIntroAnimations = true;

        private EventSystem _eventSystem;
        private WrapGroup _wrapGroup;

        protected override IReadOnlyObservableList<TagModel> Items => ViewModel.Tags;

        private void Awake()
        {
            _eventSystem = EventSystem.current;
            _wrapGroup = _itemsContainer.GetComponent<WrapGroup>();
            _itemsContainer.gameObject.SetActive(false);
        }

        protected override void OnViewModelBound()
        {
            // Dont' call base.OnViewModelBound() here, because we handle this ourselves:
            ViewModel.Tags.CollectionChanged += SearchedTagsChanged;
            ViewModel.AutoCompletionSuggestions.CollectionChanged += AutoCompletionSuggestionsChanged;
            _searchInputField.onEndEdit.AddListener(OnSearchEditEnd);
            
            ViewModel.CurrentInput.ValueChanged += str =>_searchInputField.text = str;
        }

        protected override void OnChildViewInstantiated(TagView view)
        {
            view.PlayIntroAnimation = _playIntroAnimations;
        }

        private bool IsSelected => _eventSystem.currentSelectedGameObject == _searchInputField.gameObject;
        private bool ContainsNoText => string.IsNullOrEmpty(_searchInputField.text);
        private bool _wasEmptyBeforeFrame = true;

        private void Update()
        {
            ViewModel.CurrentInput.Value = _searchInputField.text;
            if (IsSelected) OnSelected();
            else if (IsShortCutActive) SelectSearchField();

            _wasEmptyBeforeFrame = ContainsNoText;
        }

        private static bool IsShortCutActive => KeyCode.T.Down() && (LeftControl.Pressed() || RightControl.Pressed());

        private void OnSelected()
        {
            if (ContainsNoText && _wasEmptyBeforeFrame && Backspace.Down())
            {
                RemoveLastTag();
            }

            if (_autocompleteContainer.childCount > 0 && DownArrow.Down())
            {
                // Dirty trick to fix skipping over first button
                Input.ResetInputAxes();
                Select(_autocompleteContainer.GetChild(0).GetComponent<Button>().gameObject);
            }
        }

        private void RemoveLastTag()
        {
            if (_itemsContainer.childCount == 0) return;

            var child = _itemsContainer.GetChild(_itemsContainer.childCount - 1);
            var tagView = child.GetComponent<TagView>();
            tagView.OnButtonClick();
        }

        private void OnSearchEditEnd(string s)
        {
            if (Return.Down() || KeypadEnter.Down())
            {
                ViewModel.PinCurrentInputCommand.Execute();
            }
        }

        private void SelectSearchField()
        {
            Select(_searchInputField.gameObject);
            _searchInputField.OnPointerClick(new PointerEventData(_eventSystem));
        }

        private void Select(GameObject itemToSelect)
        {
            _eventSystem.SetSelectedGameObject(itemToSelect, null);
        }

        private void SearchedTagsChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            _itemsContainer.gameObject.SetActive(true);

            var hasTags = ViewModel.Tags.Any();
            var fadeIn = _itemsContainer.childCount == 0 && hasTags;
            var fadeOut = _itemsContainer.childCount != 0 && !hasTags;

            UpdateDisplayedItems(sender, args);

            if (fadeIn)
            {
                DOTween.To(() => _wrapGroup.padding.bottom, value => _wrapGroup.padding.bottom = value, 25,
                    PanelFadeDuration);
                DOTween.To(() => _wrapGroup.Spacing, value => _wrapGroup.Spacing = value, 15, PanelFadeDuration);
            }
            else if (fadeOut)
            {
                var doPadding = DOTween.To(() => _wrapGroup.padding.bottom, value => _wrapGroup.padding.bottom = value,
                    0, PanelFadeDuration);
                var doSpacing = DOTween.To(() => _wrapGroup.Spacing, value => _wrapGroup.Spacing = value, 0,
                    PanelFadeDuration);

                DOTween.Sequence()
                    .Join(doPadding).Join(doSpacing)
                    .AppendCallback(() => _itemsContainer.gameObject.SetActive(false));
            }

            SelectSearchField();
        }

        private void AutoCompletionSuggestionsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            foreach (Transform child in _autocompleteContainer)
            {
                Destroy(child.gameObject);
            }

            var hasSuggestions = ViewModel.AutoCompletionSuggestions.Any();
            _autocompleteParent.gameObject.SetActive(hasSuggestions);

            if (!hasSuggestions) return;

            foreach (var suggestion in ViewModel.AutoCompletionSuggestions)
            {
                var suggestionView = Instantiate(_suggestionPrefab, _autocompleteContainer);
                suggestionView.BindTo(suggestion);
            }
        }
    }
}