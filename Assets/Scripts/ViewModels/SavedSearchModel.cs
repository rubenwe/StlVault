using System;
using System.Collections.Generic;
using System.Windows.Input;
using JetBrains.Annotations;
using StlVault.Config;
using StlVault.Util;
using StlVault.Util.Commands;

namespace StlVault.ViewModels
{
    internal class SavedSearchModel : ModelBase
    {
        internal SavedSearchConfig Config { get; }
        public string Alias => Config.Alias;
        public IReadOnlyList<string> Tags => Config.Tags;

        public ICommand SelectCommand { get; }
        public ICommand DeleteCommand { get; }

        public SavedSearchModel(
            [NotNull] SavedSearchConfig config,
            [NotNull] Action<SavedSearchModel> selectCallback,
            [NotNull] Action<SavedSearchModel> deleteCallback)
        {
            if (selectCallback is null) throw new ArgumentNullException(nameof(selectCallback));
            Config = config ?? throw new ArgumentNullException(nameof(config));
            SelectCommand = new DelegateCommand(() => selectCallback(this));
            DeleteCommand = new DelegateCommand(() => deleteCallback(this));
        }
    }
}