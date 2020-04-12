using System;
using System.Linq;
using JetBrains.Annotations;
using StlVault.Messages;
using StlVault.Services;

namespace StlVault.ViewModels
{
    internal class EditScreenModel : DialogModelBase<RequestShowDialogMessage.EditScreen>
    {
        public ItemSelectorModel ItemSelectorModel { get; }
        public EditMenuModel EditMenuModel { get; }
        public ViewPortModel ViewPortModel { get; }
        
        public EditScreenModel([NotNull] ILibrary library)
        {
            if(library is null) throw new ArgumentNullException(nameof(library));
            
            ItemSelectorModel = new ItemSelectorModel();
            ViewPortModel = new ViewPortModel(ItemSelectorModel, library);
            EditMenuModel = new EditMenuModel(ItemSelectorModel, library);
        }

        protected override void OnAccept() { }

        protected override void Reset(bool closing)
        {
            if (closing)
            {
                ItemSelectorModel.Selected.Clear();
                ItemSelectorModel.Models.Clear();
            }
        }

        protected override void OnShown(RequestShowDialogMessage.EditScreen message)
        {
            ItemSelectorModel.Models.AddRange(message.SelectedItems);
            ItemSelectorModel.Selected.Add(message.SelectedItems.First());
        }
    }
}