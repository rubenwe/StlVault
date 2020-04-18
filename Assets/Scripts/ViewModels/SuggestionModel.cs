using System;
using System.Windows.Input;
using StlVault.Util.Commands;

namespace StlVault.ViewModels
{
    internal class SuggestionModel
    {
        public string Text { get; }
        public ICommand SelectSuggestionCommand { get; }

        public SuggestionModel(string text, Action<SuggestionModel> suggestionSelectedCallback)
        {
            Text = text;
            SelectSuggestionCommand = new DelegateCommand(() => suggestionSelectedCallback(this));
        }
    }
}