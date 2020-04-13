using System;
using JetBrains.Annotations;
using StlVault.Services;

namespace StlVault.ViewModels
{
    internal class EditMenuModel
    {
        private readonly ItemSelectorModel _selector;
        private readonly ILibrary _library;
        
        public EditMenuModel(
            [NotNull] ItemSelectorModel selector, 
            [NotNull] ILibrary library)
        {
            _selector = selector ?? throw new ArgumentNullException(nameof(selector));
            _library = library ?? throw new ArgumentNullException(nameof(library));
        }
    }
}