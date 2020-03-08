using System.ComponentModel;
using UnityEngine;

namespace StlVault.Views
{
    public abstract class ViewBase<T> : MonoBehaviour, IView<T> where T : class, INotifyPropertyChanged
    {
        public T ViewModel { get; private set; }

        public void BindTo(T viewModel)
        {
            Debug.Assert(gameObject.activeInHierarchy, "GameObjects should be active while binding!");
            
            ViewModel = viewModel;
            ViewModel.PropertyChanged += ViewModelOnPropertyChanged;
            
            OnViewModelBound();
        }
        
        public void Unbind()
        {
            ViewModel.PropertyChanged -= ViewModelOnPropertyChanged;
            ViewModel = null;

            OnViewModelUnbound();
        }
        
        private void ViewModelOnPropertyChanged(object sender, PropertyChangedEventArgs e) 
            => OnViewModelPropertyChanged(e.PropertyName);

        protected virtual void OnViewModelPropertyChanged(string propertyName) { }
        protected virtual void OnViewModelBound() {}
        protected virtual void OnViewModelUnbound() { }
    }
}
