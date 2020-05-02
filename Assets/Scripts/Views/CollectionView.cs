using StlVault.ViewModels;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

#pragma warning disable 0649

namespace StlVault.Views
{
    internal class CollectionView : ViewBase<CollectionModel>, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private Button _button;
        [SerializeField] private Button _addButton;
        [SerializeField] private Button _deleteButton;
        [SerializeField] private TMP_Text _text;

        protected override void OnViewModelBound()
        {
            _text.BindTo(ViewModel.Label, "- {0}");
            
            _addButton.BindTo(ViewModel.AddCommand);
            _button.BindTo(ViewModel.SelectCommand);
            _deleteButton.BindTo(ViewModel.DeleteCommand);
        }
        
        public void OnPointerEnter(PointerEventData eventData)
        {
            if (ViewModel.Config.Name != "Selected")
            {
                _addButton.gameObject.SetActive(true);
            }
            
            _deleteButton.gameObject.SetActive(true);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _addButton.gameObject.SetActive(false);
            _deleteButton.gameObject.SetActive(false);
        }
    }
}