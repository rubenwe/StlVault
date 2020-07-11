namespace StlVault.Config
{
    internal class LayoutSettings
    {
        public MainMenuSettings MainMenu { get; } = new MainMenuSettings();

        internal class MainMenuSettings
        {
            public bool ImportFoldersExpanded { get; set; }
            public bool SavedSearchesExpanded { get; set; }
            public bool CollectionsExpanded { get; set; }
        }
    }
}