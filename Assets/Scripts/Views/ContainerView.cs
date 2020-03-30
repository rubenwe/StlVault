using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using StlVault.Util;
using StlVault.Util.Collections;
using UnityEngine;
using NotifyCollectionChangedAction = System.Collections.Specialized.NotifyCollectionChangedAction;
using NotifyCollectionChangedEventArgs = System.Collections.Specialized.NotifyCollectionChangedEventArgs;

#pragma warning disable 0649

namespace StlVault.Views
{
    internal abstract class ContainerView<TModel, TChildView, TChildModel> : ViewBase<TModel>
        where TModel : class, INotifyPropertyChanged
        where TChildView : ViewBase<TChildModel>
        where TChildModel : class, INotifyPropertyChanged
    {
        [SerializeField] private TChildView _itemPrefab;
        [SerializeField] protected Transform _itemsContainer;
        private CancellationTokenSource _source;


        protected abstract IReadOnlyObservableList<TChildModel> Items { get; }

        protected override void OnViewModelBound() => Items.OnMainThread().CollectionChanged += UpdateDisplayedItems;

        protected async void UpdateDisplayedItems(object sender, NotifyCollectionChangedEventArgs args)
        {
            _source?.Cancel();
            _source = new CancellationTokenSource();
            var token = _source.Token;

            if (args.Action == NotifyCollectionChangedAction.Reset) await DestroyAndRecreateAllItems(token);
            else if (args.Action == NotifyCollectionChangedAction.Add)
                await AddNewItems(args.NewItems.OfType<TChildModel>().ToList(), token);
            else if (args.Action == NotifyCollectionChangedAction.Remove)
                await RemoveOldItems(args.OldItems.OfType<TChildModel>().ToHashSet(), token);
        }

        private async Task AddNewItems(IReadOnlyList<TChildModel> newViewModels, CancellationToken token)
        {
            await newViewModels.ChunkedForEach(viewModel =>
            {
                var view = Instantiate(_itemPrefab, _itemsContainer);
                view.BindTo(viewModel);
            }, token);
        }

        private async Task RemoveOldItems(HashSet<TChildModel> oldViewModels, CancellationToken token)
        {
            var children = _itemsContainer.Cast<Transform>().ToList();
            await children.ChunkedForEach(child =>
            {
                var view = child.GetComponent<TChildView>();
                if (oldViewModels.Contains(view.ViewModel)) Destroy(child.gameObject);
            }, token);
        }

        private async Task DestroyAndRecreateAllItems(CancellationToken token)
        {
            var children = _itemsContainer.Cast<Transform>().ToList();
            await children.ChunkedForEach(child => Destroy(child.gameObject), token);

            await AddNewItems(Items, token);
        }
    }
}