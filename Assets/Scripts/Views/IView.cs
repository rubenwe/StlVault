namespace StlVault.Views
{
    public interface IView<T>
    {
        T ViewModel { get; }
        void BindTo(T viewModel);
    }
}