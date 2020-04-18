using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using JetBrains.Annotations;
using NotifyCollectionChangedAction = System.Collections.Specialized.NotifyCollectionChangedAction;
using NotifyCollectionChangedEventArgs = System.Collections.Specialized.NotifyCollectionChangedEventArgs;
using NotifyCollectionChangedEventHandler = System.Collections.Specialized.NotifyCollectionChangedEventHandler;

namespace StlVault.Util.Collections
{
    /// <summary>
    /// A collection that tracks changes in the base collection and reflects them.
    /// </summary>
    /// <typeparam name="TFrom">The type of the source items.</typeparam>
    /// <typeparam name="TTo">The type of the the target items to create.</typeparam>
    /// <seealso cref="IReadOnlyObservableCollection{T}" />
    public class TrackingCollection<TFrom, TTo> : IReadOnlyObservableList<TTo>
    {
        [CanBeNull] private IReadOnlyObservableList<TFrom> _sourceItems;
        
        private readonly Func<TFrom, TTo> _createTargetItemFunc;
        private readonly Dictionary<TFrom, TTo> _lookup = new Dictionary<TFrom, TTo>();
        private readonly ObservableList<TTo> _targetItems = new ObservableList<TTo>();
        private readonly IReadOnlyObservableCollection<TTo> _targetItemsWrapper;

        public TrackingCollection([NotNull] Func<TFrom, TTo> createTargetItemFunc) : this(null, createTargetItemFunc) {}
        
        public TrackingCollection(
            [CanBeNull] IReadOnlyObservableList<TFrom> sourceItems,
            [NotNull] Func<TFrom, TTo> createTargetItemFunc)
        {
            _sourceItems = sourceItems;
            _createTargetItemFunc = createTargetItemFunc ?? throw new ArgumentNullException(nameof(createTargetItemFunc));
            _targetItemsWrapper = new ReadOnlyObservableCollectionWrapper<TTo, TTo>(_targetItems);

            if (sourceItems != null) SetSource(sourceItems);
        }

        public void SetSource([NotNull] IReadOnlyObservableList<TFrom> newSource)
        {
            if (newSource == null) throw new ArgumentNullException(nameof(newSource));

            using (_targetItems.EnterMassUpdate())
            {
                if (_sourceItems != null)
                {
                    _sourceItems.CollectionChanged -= SourceItemsOnCollectionChanged;
                    if (_sourceItems is IDisposable disposable) disposable.Dispose();
                }

                _sourceItems = newSource;
                _sourceItems.CollectionChanged += SourceItemsOnCollectionChanged;
                
                _targetItems.Clear();
                _lookup.Clear();
                
                AddNewItems(newSource);
            }
        }

        private void SourceItemsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            var newSourceItems = args.NewItems?.Cast<TFrom>().ToList();
            var oldSourceItems = args.OldItems?.Cast<TFrom>().ToList();

            switch (args.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    AddNewItems(newSourceItems);
                    break;

                case NotifyCollectionChangedAction.Remove:
                    RemoveOldItems(oldSourceItems);
                    break;
                case NotifyCollectionChangedAction.Reset:
                    SetSource((IReadOnlyObservableList<TFrom>) sender);
                    break;
                case NotifyCollectionChangedAction.Replace:
                case NotifyCollectionChangedAction.Move:
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void RemoveOldItems(IEnumerable<TFrom> oldSourceItems)
        {
            using (_targetItems.EnterMassUpdate())
            {
                foreach (var oldSourceItem in oldSourceItems)
                {
                    if (_lookup.TryGetValue(oldSourceItem, out var oldTargetItem))
                    {
                        _targetItems.Remove(oldTargetItem);
                        _lookup.Remove(oldSourceItem);
                    }
                }
            }
        }

        private void AddNewItems(IEnumerable<TFrom> newSourceItems)
        {
            using (_targetItems.EnterMassUpdate())
            {
                foreach (var newSourceItem in newSourceItems)
                {
                    var newTargetItem = _createTargetItemFunc(newSourceItem);
                    _targetItems.Add(newTargetItem);
                    _lookup[newSourceItem] = newTargetItem;
                }
            }
        }

        public IEnumerator<TTo> GetEnumerator()
        {
            return _targetItemsWrapper.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable) _targetItemsWrapper).GetEnumerator();
        }

        public int Count => _targetItemsWrapper.Count;
        public IReadOnlyObservableList<TFrom> Source => _sourceItems;

        public event PropertyChangedEventHandler PropertyChanged
        {
            add => _targetItemsWrapper.PropertyChanged += value;
            remove => _targetItemsWrapper.PropertyChanged -= value;
        }

        public event NotifyCollectionChangedEventHandler CollectionChanged
        {
            add => _targetItemsWrapper.CollectionChanged += value;
            remove => _targetItemsWrapper.CollectionChanged -= value;
        }

        public TTo this[int index] => _targetItems[index];
    }
}