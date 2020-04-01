using StlVault.ViewModels;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

#pragma warning disable 0649

namespace StlVault.Views
{
    internal class AddSavedSearchDialog : DialogView<AddSavedSearchModel>
    {
        [SerializeField] private TMP_InputField _aliasField;

        private EventSystem _eventSystem;

        private void Start()
        {
            _eventSystem = EventSystem.current;
        }

        protected override void OnViewModelBound()
        {
            base.OnViewModelBound();
            _aliasField.onValueChanged.AddListener(text => ViewModel.Alias = text);
        }

        protected override void OnViewModelPropertyChanged(string propertyName)
        {
            base.OnViewModelPropertyChanged(propertyName);
            switch (propertyName)
            {
                case nameof(ViewModel.Alias):
                    _aliasField.text = ViewModel.Alias;
                    break;
                case nameof(ViewModel.Shown) when ViewModel.Shown:
                    _eventSystem.SetSelectedGameObject(_aliasField.gameObject, null);
                    break;
            }
        }
    }
}