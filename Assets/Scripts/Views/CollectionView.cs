using StlVault.ViewModels;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

#pragma warning disable 0649

namespace StlVault.Views
{
    internal class CollectionView : ViewBase<CollectionModel>
    {
        [SerializeField] private Button _button;
        [SerializeField] private TMP_Text _text;

        protected override void OnViewModelBound()
        {
            _text.BindTo(ViewModel.Name);
        }
    }
}