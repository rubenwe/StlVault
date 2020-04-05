using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using NotifyCollectionChangedAction = System.Collections.Specialized.NotifyCollectionChangedAction;
using NotifyCollectionChangedEventArgs = System.Collections.Specialized.NotifyCollectionChangedEventArgs;
using NotifyCollectionChangedEventHandler = System.Collections.Specialized.NotifyCollectionChangedEventHandler;

namespace StlVault.Util.Collections
{
    public class ObservableSet<T> : IReadOnlyObservableCollection<T>
    {
        private readonly HashSet<T> _items = new HashSet<T>();
        public IEnumerator<T> GetEnumerator() => _items.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        public int Count => _items.Count;

        public event PropertyChangedEventHandler PropertyChanged;
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        public bool Add(T item)
        {
            if (!_items.Add(item)) return false;
            
            var args = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, new[]{item});
            PropertyChanged?.Invoke(this, PropertyChangedEventArgsCache.Get(nameof(Count)));
            CollectionChanged?.Invoke(this, args);
            
            return true;
        }

        public bool Remove(T item)
        {
            if (!_items.Remove(item)) return false;

            var args = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, new[]{item});
            PropertyChanged?.Invoke(this, PropertyChangedEventArgsCache.Get(nameof(Count)));
            CollectionChanged?.Invoke(this, args);

            return true;
        }

        public bool Contains(T item) => _items.Contains(item);
    }
}