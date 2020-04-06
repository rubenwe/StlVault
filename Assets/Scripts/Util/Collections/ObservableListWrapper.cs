using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using NotifyCollectionChangedAction = System.Collections.Specialized.NotifyCollectionChangedAction;
using NotifyCollectionChangedEventArgs = System.Collections.Specialized.NotifyCollectionChangedEventArgs;
using NotifyCollectionChangedEventHandler = System.Collections.Specialized.NotifyCollectionChangedEventHandler;

namespace StlVault.Util.Collections
{
    internal class ObservableListWrapper<T> : IReadOnlyObservableList<T>
    {
        private IReadOnlyObservableList<T> _inner;

        public void SetSource(IReadOnlyObservableList<T> newInner)
        {
            if (_inner != null)
            {
                _inner.PropertyChanged -= InnerOnPropertyChanged;
                _inner.CollectionChanged -= InnerCollectionChanged;
            }
            
            _inner = newInner;
            newInner.PropertyChanged += InnerOnPropertyChanged;
            newInner.CollectionChanged += InnerCollectionChanged;

            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        private void InnerCollectionChanged(object sender, NotifyCollectionChangedEventArgs args) => CollectionChanged?.Invoke(this, args);
        private void InnerOnPropertyChanged(object sender, PropertyChangedEventArgs args) => PropertyChanged?.Invoke(this, args);

        public IEnumerator<T> GetEnumerator()
        {
            return _inner.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable) _inner).GetEnumerator();
        }

        public int Count => _inner.Count;

        public event PropertyChangedEventHandler PropertyChanged;
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        public T this[int index] => _inner[index];
    }
}