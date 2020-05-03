using StlVault.Util.Unity;
using StlVault.ViewModels;
using TMPro;
using UnityEngine;

#pragma warning disable 0649

namespace StlVault.Views
{
    internal class ViewPanel : ViewBase<ViewPanelModel>
    {
        [SerializeField] private SimpleButton _showInExplorerButton;
        [SerializeField] private SimpleButton _openIn3DViewButton;
        
        protected override void OnViewModelBound()
        {
            _showInExplorerButton.BindTo(ViewModel.ShowInExplorerCommand);
            _openIn3DViewButton.BindTo(ViewModel.OpenIn3DViewCommand);
            
            _showInExplorerButton.GetComponentInChildren<TMP_Text>().BindTo(ViewModel.LauncherName, "Show in {0}");
        }
    }
}