using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using StlVault.Services;
using StlVault.Util.Collections;
using UnityEngine;

namespace StlVault.ViewModels
{
    internal class ViewPortModel
    {
        public ObservableList<Mesh> Meshes { get; } = new ObservableList<Mesh>();
        private readonly Dictionary<ItemPreviewModel, Mesh> _lookup = new Dictionary<ItemPreviewModel, Mesh>();
        private readonly ItemSelectorModel _selector;
        private readonly ILibrary _library;
        
        public ViewPortModel([NotNull] ItemSelectorModel selector, [NotNull] ILibrary library)
        {
            _selector = selector ?? throw new ArgumentNullException(nameof(selector));
            _library = library ?? throw new ArgumentNullException(nameof(library));
            
            selector.Selected.CollectionChanged += SelectedOnCollectionChanged;
        }

        private async void SelectedOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (var previewModel in e.NewItems.OfType<ItemPreviewModel>())
                    {
                        if (_lookup.TryGetValue(previewModel, out _)) continue;
                        await AddMeshAsync(previewModel);
                    }
                    
                    break;
               
                case NotifyCollectionChangedAction.Remove:
                    foreach (var previewModel in e.OldItems.OfType<ItemPreviewModel>())
                    {
                        if (_lookup.TryGetValue(previewModel, out var mesh))
                        {
                            _lookup.Remove(previewModel);
                            Meshes.Remove(mesh);
                        }
                    }
                    
                    break;
                case NotifyCollectionChangedAction.Reset:
                    _lookup.Clear();
                    Meshes.Clear();
                    foreach (var model in _selector.Selected)
                    {
                        await AddMeshAsync(model);
                    }

                    break;
            }
        }

        private async Task AddMeshAsync(ItemPreviewModel model)
        {
            var mesh = await _library.GetMeshAsync(model);
            _lookup.Add(model, mesh);
            Meshes.Add(mesh);
        }
    }
}