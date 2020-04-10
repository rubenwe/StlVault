using StlVault.ViewModels;
using UnityEngine;
using UnityEngine.UI;

#pragma warning disable 0649

namespace StlVault.Views
{
    internal class ApplicationView : ViewBase<ApplicationModel>
    {
        [SerializeField] private Button _showSettingsButton;
        [SerializeField] private Button _showFeedbackButton;
        [SerializeField] private Button _showHelpButton;

        protected override void OnViewModelBound()
        {
            base.OnViewModelBound();

            _showSettingsButton.Bind(ViewModel.ShowAppSettingsCommand);
            _showFeedbackButton.Bind(ViewModel.ShowFeedbackCommand);
            _showHelpButton.Bind(ViewModel.ShowHelpCommand);
        }
    }
}