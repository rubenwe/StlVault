using StlVault.Util.Commands;
using StlVault.ViewModels;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

#pragma warning disable 0649

namespace StlVault.Views
{
    internal class SavedSearchView : ViewBase<SavedSearchModel>, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private Button _selectButton;
        [SerializeField] private Button _deleteButton;
        [SerializeField] private TMP_Text _text;

        protected override void OnViewModelBound()
        {
            _selectButton.onClick.AddListener(ViewModel.SelectCommand.Execute);
            _deleteButton.onClick.AddListener(ViewModel.DeleteCommand.Execute);
            _text.text = "- " + ViewModel.Alias;
        }

        public void OnPointerEnter(PointerEventData eventData) => _deleteButton.gameObject.SetActive(true);
        public void OnPointerExit(PointerEventData eventData) => _deleteButton.gameObject.SetActive(false);
    }
}