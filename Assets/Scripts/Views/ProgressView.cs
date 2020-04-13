using StlVault.ViewModels;
using TMPro;
using UnityEngine;

#pragma warning disable 0649

namespace StlVault.Views
{
    [RequireComponent(typeof(TMP_Text))]
    internal class ProgressView : ViewBase<ProgressModel>
    {
        private TMP_Text _text;

        private void Awake()
        {
            _text = GetComponent<TMP_Text>();
        }

        protected override void OnViewModelBound()
        {
            ViewModel.ProgressText.OnMainThread().ValueChanged += OnValueChanged;
        }

        private void OnValueChanged(string newText)
        {
            _text.text = newText;
        }
    }
}