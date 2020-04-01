using System;
using System.Windows.Input;
using StlVault.Util;
using StlVault.Util.Commands;

namespace StlVault.ViewModels
{
    internal class TagModel : ModelBase
    {
        public string Text { get; }
        public ICommand RemoveCommand { get; }

        public TagModel(string text, Action<TagModel> removeCallback)
        {
            Text = text;
            RemoveCommand = new DelegateCommand(() => removeCallback(this));
        }
    }
}