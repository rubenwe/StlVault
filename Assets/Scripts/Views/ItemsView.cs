using StlVault.Util.Collections;
using StlVault.ViewModels;
using UnityEngine;

namespace StlVault.Views
{
    internal class ItemsView : ContainerView<ItemsModel, ItemView, ItemPreviewModel>
    {
        protected override IReadOnlyObservableList<ItemPreviewModel> ChildModels => ViewModel.Items;

        private void Update()
        {
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
        }
    }
}