using System.Windows.Input;
using StlVault.Config;
using StlVault.Services;
using StlVault.Util;
using StlVault.Util.Commands;

namespace StlVault.ViewModels
{
    internal class CollectionModel
    {
        public DelegateProperty<string> Label { get; }
        
        private BindableProperty<string> Name { get; } = new BindableProperty<string>();
        private DelegateProperty<int> Count { get; }
        
        public ICommand SelectCommand { get; }
        public ICommand AddCommand { get; }
        public ICommand DeleteCommand { get; }
        public CollectionConfig Config { get; }

        public CollectionModel(CollectionConfig config, IPreviewList stream, CollectionCommands commands)
        {
            Config = config;
            Name.Value = config.Name;
         
            DelegateCommand MakeCommand(ICommand command) => 
                new DelegateCommand(() => command.CanExecute(this), () => command.Execute(this)).UpdateOn(command);
            
            AddCommand = MakeCommand(commands.Add);
            SelectCommand = MakeCommand(commands.Select);
            DeleteCommand = MakeCommand(commands.Delete);
            
            Count = new DelegateProperty<int>(() => stream.Count)
                .UpdateOn(stream);
            
            Label = new DelegateProperty<string>(() => $"{Name.Value} ({Count.Value})")
                .UpdateOn(Name)
                .UpdateOn(Count);
        }
    }
}