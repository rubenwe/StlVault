using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using StlVault.Util.Collections;
using NotifyCollectionChangedEventHandler = System.Collections.Specialized.NotifyCollectionChangedEventHandler;

namespace StlVault.Util.Unity
{
    internal class GuiThreadQueuedReadOnlyObservableList<T> : IReadOnlyObservableList<T>
    {
        private readonly IReadOnlyObservableList<T> _list;
        
        private event PropertyChangedEventHandler GuiPropertyChanged;
        private event NotifyCollectionChangedEventHandler GuiCollectionChanged;
        
        public GuiThreadQueuedReadOnlyObservableList(IReadOnlyObservableList<T> list)
        {
            _list = list;
            _list.PropertyChanged += (s, a) => GuiCallbackQueue.Enqueue(() => GuiPropertyChanged?.Invoke(s, a));
            _list.CollectionChanged += (s, a) => GuiCallbackQueue.Enqueue(() => GuiCollectionChanged?.Invoke(s, a));
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable) _list).GetEnumerator();
        }

        public int Count => _list.Count;

        public event PropertyChangedEventHandler PropertyChanged
        {
            add => GuiPropertyChanged += value;
            remove => GuiPropertyChanged -= value;
        }

        public event NotifyCollectionChangedEventHandler CollectionChanged
        {
            add => GuiCollectionChanged += value;
            remove => GuiCollectionChanged -= value;
        }

        public T this[int index] => _list[index];
    }
}