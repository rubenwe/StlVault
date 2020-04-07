using System;
using System.ComponentModel;
using System.Windows.Input;
using StlVault.Util.Commands;

namespace StlVault.ViewModels
{
    internal class TagModel 
    {
        public string Text { get; }
        public bool IsPartial { get; set; }
        public ICommand RemoveCommand { get; }

        public TagModel(string text, Action<TagModel> removeCallback)
        {
            Text = text;
            RemoveCommand = new DelegateCommand(() => removeCallback(this));
        }
    }
}