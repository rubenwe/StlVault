using StlVault.Util.Collections;
using StlVault.ViewModels;
using StlVault.Views;
using UnityEngine;

namespace StlVault.Util.Unity
{
    internal class LibraryView : VirtualGridLayoutGroup<ItemsModel, ItemView, ItemPreviewModel>
    {
        protected override IReadOnlyObservableList<ItemPreviewModel> ChildModels => ViewModel?.Items;
        
        protected override void Update()
        {
            if (ViewModel == null) return;
            
            var ctrl = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
            if (ctrl && Input.GetKeyDown(KeyCode.A))
            {
                ViewModel.ToggleAll();
            }

            if (ctrl && Input.GetKeyDown(KeyCode.D))
            {
                ViewModel.ClearSelection();
            }

            var shift = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
            ViewModel.SelectRange = shift;
            
            base.Update();
        }
    }
}