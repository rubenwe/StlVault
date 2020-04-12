using System.Collections.Generic;
using StlVault.ViewModels;

namespace StlVault.Messages
{
    internal static class RequestShowDialogMessage
    {
        public struct AppSettings { }
        public struct ExitingDialog { }
        internal struct AddImportFolder { }
        internal struct AddCollection { }
        internal struct UserFeedback { }
        
        public class SaveSearch
        {
            public IReadOnlyList<string> SearchTags { get; set; }
        }

        internal class EditScreen
        {
            public IReadOnlyList<ItemPreviewModel> SelectedItems { get; set; }
        }
    }
}