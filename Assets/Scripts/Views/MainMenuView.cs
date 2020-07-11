using StlVault.Util.Unity;

namespace StlVault.Views
{
    internal abstract class MainMenuView<TModel, TChildView, TChildModel> : ContainerView<TModel, TChildView, TChildModel>
        where TModel : class
        where TChildView : ViewBase<TChildModel>
        where TChildModel : class
    {
        private Accordion _accordion;

        public bool Expanded
        {
            get => _accordion.IsExpanded;
            set => _accordion.IsExpanded = value;
        }

        private void Awake()
        {
            _accordion = GetComponent<Accordion>();
        }
    }
}