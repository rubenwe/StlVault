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
            _aliasField.BindTo(ViewModel.Alias);
        }

        protected override void OnShownChanged(bool shown)
        {
            base.OnShownChanged(shown);
            
            if (shown)
            {
                _eventSystem.SetSelectedGameObject(_aliasField.gameObject, null);
            }
        }
    }
}