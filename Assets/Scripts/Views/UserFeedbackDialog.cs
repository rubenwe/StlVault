using StlVault.ViewModels;
using UnityEngine;
using UnityEngine.UI;

#pragma warning disable 0649

namespace StlVault.Views
{
    internal class UserFeedbackDialog : DialogView<UserFeedbackModel>
    {
        [SerializeField] private Toggle _includeDataToggle;
        [SerializeField] private Button _createArchiveButton;
        [SerializeField] private Button _createIssueButton;
        [SerializeField] private Button _openFeatureRequestsButton;
        [SerializeField] private Button _joinDiscordButton;
        
        protected override void OnViewModelBound()
        {
            base.OnViewModelBound();
            
            _includeDataToggle.Bind(ViewModel.IncludeUserData);
            _createArchiveButton.Bind(ViewModel.CreateArchiveCommand);
            _createIssueButton.Bind(ViewModel.CreateIssueCommand);
            _openFeatureRequestsButton.Bind(ViewModel.OpenFeatureRequestsCommand);
            _joinDiscordButton.Bind(ViewModel.JoinDiscordCommand);
        }
    }
}