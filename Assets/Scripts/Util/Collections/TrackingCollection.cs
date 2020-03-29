using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    public class TrackingCollection<TFrom, TTo> : IReadOnlyObservableCollection<TTo>
    {
        private readonly Func<TFrom, TTo> _createTargetItemFunc;
        private readonly Dictionary<TFrom, TTo> _lookup = new Dictionary<TFrom, TTo>();
        private readonly ObservableCollection<TTo> _targetItems = new ObservableCollection<TTo>();
        private readonly IReadOnlyObservableCollection<TTo> _targetItemsWrapper;

        public TrackingCollection(
            [NotNull] IReadOnlyObservableCollection<TFrom> sourceItems,
            [NotNull] Func<TFrom, TTo> createTargetItemFunc)
        {
            if (sourceItems == null) throw new ArgumentNullException(nameof(sourceItems));

            _createTargetItemFunc =
                createTargetItemFunc ?? throw new ArgumentNullException(nameof(createTargetItemFunc));
            sourceItems.CollectionChanged += SourceItemsOnCollectionChanged;
            _targetItemsWrapper = new ReadOnlyObservableCollectionWrapper<TTo, TTo>(_targetItems);

            AddNewItems(sourceItems);
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

                case NotifyCollectionChangedAction.Replace:
                    break;
                case NotifyCollectionChangedAction.Move:
                    break;
                case NotifyCollectionChangedAction.Reset:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void RemoveOldItems(IEnumerable<TFrom> oldSourceItems)
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

        private void AddNewItems(IEnumerable<TFrom> newSourceItems)
        {
            foreach (var newSourceItem in newSourceItems)
            {
                var newTargetItem = _createTargetItemFunc(newSourceItem);
                _targetItems.Add(newTargetItem);
                _lookup[newSourceItem] = newTargetItem;
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
    }
}