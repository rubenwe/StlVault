using StlVault.Messages;
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
            _showSettingsButton.BindTo(ViewModel.ShowAppSettingsCommand);
            _showFeedbackButton.BindTo(ViewModel.ShowFeedbackCommand);
            _showHelpButton.BindTo(ViewModel.ShowHelpCommand);
        }
    }
}