using System.Collections.Generic;

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
    }
}