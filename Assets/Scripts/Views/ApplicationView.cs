using StlVault.ViewModels;
using UnityEngine;
using UnityEngine.UI;

#pragma warning disable 0649

namespace StlVault.Views
{
    internal class ApplicationView : ViewBase<ApplicationModel>
    {
        [SerializeField] private Button _showSettingsButton;

        protected override void OnViewModelBound()
        {
            base.OnViewModelBound();

            _showSettingsButton.Bind(ViewModel.ShowAppSettingsCommand);
        }
    }
}