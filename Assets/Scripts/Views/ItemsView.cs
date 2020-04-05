using StlVault.Util.Collections;
using StlVault.ViewModels;
using UnityEngine;

namespace StlVault.Views
{
    internal class ItemsView : ContainerView<ItemsModel, ItemView, FilePreviewModel>
    {
        protected override IReadOnlyObservableList<FilePreviewModel> Items => ViewModel.Items;

        private void Update()
        {
            var ctrl = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
            if (ctrl && Input.GetKeyDown(KeyCode.A))
            {
                ViewModel.SelectAll();
            }

            var shift = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
            ViewModel.SelectRange = shift;
        }
    }
}