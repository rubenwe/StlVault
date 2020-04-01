using StlVault.Util.Commands;
using StlVault.ViewModels;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

#pragma warning disable 0649

namespace StlVault.Views
{
    [RequireComponent(typeof(Button))]
    internal class SuggestionView : ViewBase<SuggestionModel>
    {
        private Button _button;
        private TMP_Text _text;

        private void Awake()
        {
            _button = GetComponent<Button>();
            _text = GetComponentInChildren<TMP_Text>();
        }

        protected override void OnViewModelBound()
        {
            _text.text = ViewModel.Text;
            _button.onClick.AddListener(ViewModel.SelectSuggestionCommand.Execute);
        }
    }
}