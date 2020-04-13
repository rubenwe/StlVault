using System;
using DG.Tweening;
using StlVault.Util.Unity;
using StlVault.ViewModels;
using UnityEngine;

#pragma warning disable 0649

namespace StlVault.Views
{
    internal class EditScreen : DialogView<EditScreenModel>
    {
        public CanvasGroup MainView { private get; set; }

        [SerializeField] private SimpleButton _backButton;
        [SerializeField] private EditMenu _editMenu;
        [SerializeField] private ItemSelector _itemSelector;
        [SerializeField] private ViewPort _viewPort;

        protected override void OnViewModelBound()
        {
            _editMenu.BindTo(ViewModel.EditMenuModel);
            _itemSelector.BindTo(ViewModel.ItemSelectorModel);
            _viewPort.BindTo(ViewModel.ViewPortModel);
            
            _backButton.BindTo(ViewModel.CancelCommand);
            
            base.OnViewModelBound();
        }

        protected override void OnShownChanged(bool editViewShown)
        {
            base.OnShownChanged(editViewShown);
            
            MainView.blocksRaycasts = !editViewShown;
            MainView.DOFade(editViewShown ? 0 : 1, FadeDuration);  
        }
    }
}