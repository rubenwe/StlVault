using UnityEngine;

namespace StlVault.Views
{
    public abstract class ViewBase<T> : MonoBehaviour, IView<T> where T : class
    {
        public T ViewModel { get; private set; }

        public void BindTo(T viewModel)
        {
            Debug.Assert(gameObject.activeInHierarchy, "GameObjects should be active while binding!");
            ViewModel = viewModel;
            OnViewModelBound();
        }

        protected abstract void OnViewModelBound();
    }
}