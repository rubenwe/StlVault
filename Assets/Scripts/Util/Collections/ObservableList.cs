using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using NotifyCollectionChangedAction = System.Collections.Specialized.NotifyCollectionChangedAction;
using NotifyCollectionChangedEventArgs = System.Collections.Specialized.NotifyCollectionChangedEventArgs;
using NotifyCollectionChangedEventHandler = System.Collections.Specialized.NotifyCollectionChangedEventHandler;

namespace StlVault.Util.Collections
{
    public class ObservableList<T> 
        : ObservableCollection<T>, IReadOnlyObservableList<T>
    {
        private int _massUpdateScopeCount;
        public ObservableList() {}
        
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

            void DisposeAction()
            {
                _massUpdateScopeCount--;
                if (_massUpdateScopeCount == 0)
                {
                    OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
                    OnPropertyChanged(PropertyChangedEventArgsCache.Get(nameof(Count)));
                }
            }

            return Disposable.FromAction(DisposeAction);
        }

        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (_massUpdateScopeCount == 0)
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
    }
}