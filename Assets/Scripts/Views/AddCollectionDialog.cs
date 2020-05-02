using StlVault.ViewModels;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

#pragma warning disable 0649

namespace StlVault.Views
{
    internal class AddCollectionDialog : DialogView<AddCollectionModel>
    {
        [SerializeField] private TMP_InputField _nameInput;
        private EventSystem _eventSystem;

        private void Start()
        {
            _eventSystem = EventSystem.current;
        }
        
        protected override void OnViewModelBound()
        {
            base.OnViewModelBound();
            
            _nameInput.BindTo(ViewModel.Name);
        }
        
        protected override void OnShownChanged(bool shown)
        {
            base.OnShownChanged(shown);
            
            if (shown)
            {
                _eventSystem.SetSelectedGameObject(_nameInput.gameObject, null);
            }
        }
    }
}