using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

#pragma warning disable 0649

namespace StlVault.Views
{
    internal class ItemView : ItemViewBase, IPointerClickHandler
    {
        [SerializeField] private TMP_Text _itemName;
        private GameObject _textBanner;
        
        protected override void OnViewModelBound()
        {
            _itemName.text = ViewModel.Name;

            ViewModel.Selected.ValueChanged += SelectedChanged;
            ViewModel.PreviewChanged += PreviewChanged;
            SelectedChanged(ViewModel.Selected);
        }

        protected override void OnAwake()
        {
            base.OnAwake();
            
            _textBanner = _itemName.transform.parent.gameObject;
            _textBanner.SetActive(false);
        }

        protected override void OnUpdate(bool isVisible)
        {
            base.OnUpdate(isVisible);
            
            _textBanner.SetActive(isVisible);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            ViewModel.Selected.Value = !ViewModel.Selected;
        }
    }
}