using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using NotifyCollectionChangedAction = System.Collections.Specialized.NotifyCollectionChangedAction;
using NotifyCollectionChangedEventArgs = System.Collections.Specialized.NotifyCollectionChangedEventArgs;
using NotifyCollectionChangedEventHandler = System.Collections.Specialized.NotifyCollectionChangedEventHandler;

namespace StlVault.Util.Collections
{
    public class ObservableSet<T> : IReadOnlyObservableCollection<T>
    {
        private readonly HashSet<T> _items = new HashSet<T>(); 
        private int _massUpdateScopeCount;
        private List<T> _newItems;
        private List<T> _oldItems;
        private bool _reset;
        private bool _waitingForMassUpdate;
        public IEnumerator<T> GetEnumerator() => _items.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        public int Count => _items.Count;

        public event PropertyChangedEventHandler PropertyChanged;
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        public bool Add(T item)
        {
            if (!_items.Add(item)) return false;
            
            var args = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, new[]{item});
            
            OnCollectionChanged(args);
            OnPropertyChanged(nameof(Count));
            
            return true;
        }

        public bool Remove(T item)
        {
            if (!_items.Remove(item)) return false;

            var args = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, new[]{item});
            
            OnCollectionChanged(args);
            OnPropertyChanged(nameof(Count));

            return true;
        }

        public bool Contains(T item) => _items.Contains(item);

        public IDisposable EnterMassUpdate()
        {
            _massUpdateScopeCount++;
            _waitingForMassUpdate = true;

            void DisposeAction()
            {
                _massUpdateScopeCount--;
                if (_massUpdateScopeCount == 0)
                {
                    _waitingForMassUpdate = false;

                    var args = GetChangeArguments();
                    if (args == null) return;
                    
                    OnCollectionChanged(args);
                    OnPropertyChanged(nameof(Count));
                    
                    _reset = false;
                    _newItems = null;
                    _oldItems = null;
                }
            }

            return Disposable.FromAction(DisposeAction);
        }

        private NotifyCollectionChangedEventArgs GetChangeArguments()
        {
            if (_reset) return new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset);
            if (_newItems != null && _oldItems != null) return new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset);
            if(_newItems != null) return new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, _newItems);
            if(_oldItems != null) return new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, _oldItems);

            return null;
        }

        private void OnCollectionChanged(NotifyCollectionChangedEventArgs args)
        {
            if (_massUpdateScopeCount != 0)
            {
                switch (args.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        _newItems = _newItems ?? new List<T>();
                        _newItems.AddRange(args.NewItems.OfType<T>());
                        break;
                    case NotifyCollectionChangedAction.Remove:
                        _oldItems = _oldItems ?? new List<T>();
                        _oldItems.AddRange(args.OldItems.OfType<T>());
                        break;
                    default:
                        _reset = true;
                        break;
                }
            }
            else if(!_waitingForMassUpdate)
            {
                CollectionChanged?.Invoke(this, args);
            }
        }

        private void OnPropertyChanged(string propertyName)
        {
            if (_massUpdateScopeCount == 0)
            {
                PropertyChanged?.Invoke(this, PropertyChangedEventArgsCache.Get(nameof(Count)));
            }
        }

        public void AddRange(IReadOnlyCollection<T> newItems)
        {
            using (EnterMassUpdate())
            {
                foreach (var item in newItems)
                {
                    Add(item);
                }
            }
        }
    }
}