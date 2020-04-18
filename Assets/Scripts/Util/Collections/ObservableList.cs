using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using static System.Collections.Specialized.NotifyCollectionChangedAction;
using NotifyCollectionChangedAction = System.Collections.Specialized.NotifyCollectionChangedAction;
using NotifyCollectionChangedEventArgs = System.Collections.Specialized.NotifyCollectionChangedEventArgs;
using NotifyCollectionChangedEventHandler = System.Collections.Specialized.NotifyCollectionChangedEventHandler;

namespace StlVault.Util.Collections
{
    public class ObservableList<T>
        : ObservableCollection<T>, IReadOnlyObservableList<T>
    {
        private int _massUpdateScopeCount;
        private List<T> _newItems;
        private List<T> _oldItems;
        private bool _reset;
        private bool _waitingForMassUpdate;

        public ObservableList()
        {
        }

        public ObservableList(IEnumerable<T> list)
            : base(list)
        {
        }

        public void AddRange(IEnumerable<T> items)
        {
            using (EnterMassUpdate())
            {
                foreach (var item in items)
                {
                    Add(item);
                }
            }
        }

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
                    OnPropertyChanged(PropertyChangedEventArgsCache.Get(nameof(Count)));
                    
                    _reset = false;
                    _newItems = null;
                    _oldItems = null;
                }
            }

            return Disposable.FromAction(DisposeAction);
        }

        private NotifyCollectionChangedEventArgs GetChangeArguments()
        {
            if (_reset) return new NotifyCollectionChangedEventArgs(Reset);
            if (_newItems != null && _oldItems != null) return new NotifyCollectionChangedEventArgs(Reset);
            if(_newItems != null) return new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, _newItems);
            if(_oldItems != null) return new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, _oldItems);

            return null;
        }

        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (_massUpdateScopeCount != 0)
            {
                switch (e.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        _newItems = _newItems ?? new List<T>();
                        _newItems.AddRange(e.NewItems.OfType<T>());
                        break;
                    case NotifyCollectionChangedAction.Remove:
                        _oldItems = _oldItems ?? new List<T>();
                        _oldItems.AddRange(e.OldItems.OfType<T>());
                        break;
                    default:
                        _reset = true;
                        break;
                }
            }
            else if(!_waitingForMassUpdate)
            {
                base.OnCollectionChanged(e);
            }
        }

        protected override void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (_massUpdateScopeCount == 0)
            {
                base.OnPropertyChanged(e);
            }
        }

        public new event NotifyCollectionChangedEventHandler CollectionChanged
        {
            add => base.CollectionChanged += value;
            remove => base.CollectionChanged -= value;
        }

        public new event PropertyChangedEventHandler PropertyChanged
        {
            add => base.PropertyChanged += value;
            remove => base.PropertyChanged -= value;
        }

        public void ChangeTo(IEnumerable<T> items)
        {
            using (EnterMassUpdate())
            {
                Clear();
                AddRange(items);
            }
        }
    }
}