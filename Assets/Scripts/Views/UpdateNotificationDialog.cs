using StlVault.ViewModels;
using TMPro;
using UnityEngine;

#pragma warning disable 0649

namespace StlVault.Views
{
    internal class UpdateNotificationDialog : DialogView<UpdateNotificationModel>
    {
        [SerializeField] private TMP_Text _currentVersionText;
        [SerializeField] private TMP_Text _updateVersionText;
        [SerializeField] private TMP_Text _changesText;
        
        protected override void OnViewModelBound()
        {
            base.OnViewModelBound();
            
            _currentVersionText.BindTo(ViewModel.CurrentVersion);
            _updateVersionText.BindTo(ViewModel.UpdateVersion);
            _changesText.BindTo(ViewModel.Changes);
        }
    }
}