using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using StlVault.ViewModels;
using UnityEngine;

#pragma warning disable 0649

namespace StlVault.Views
{
    internal class ViewPort : ViewBase<ViewPortModel>
    {
        [SerializeField] private Material _material;
        [SerializeField] private Transform _meshParent;
        
        private readonly Dictionary<Mesh, GameObject> _lookup = new Dictionary<Mesh, GameObject>();

        protected override void OnViewModelBound()
        {
            ViewModel.Meshes.OnMainThread().CollectionChanged += OnCollectionChanged;
        }

        private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            // ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (Mesh mesh in e.NewItems)
                    {
                        InstantiateMesh(mesh);
                    }
                    
                    break;
                
                case NotifyCollectionChangedAction.Remove:
                    foreach (Mesh mesh in e.OldItems)
                    {
                        DestroyMesh(mesh);
                    }
                    
                    break;
                
                case NotifyCollectionChangedAction.Reset:
                    foreach (var mesh in _lookup.Keys.ToList())
                    {
                        DestroyMesh(mesh);
                    }

                    break;
            }
        }

        private void DestroyMesh(Mesh mesh)
        {
            if (_lookup.TryGetValue(mesh, out var gameObj))
            {
                Destroy(gameObj);
                Destroy(mesh);
                _lookup.Remove(mesh);
            }
        }

        private void InstantiateMesh(Mesh mesh)
        {
            if (_lookup.ContainsKey(mesh)) return;
            
            var newGameObj = new GameObject(mesh.name);
            newGameObj.transform.SetParent(_meshParent);
            
            newGameObj.transform.localPosition = Vector3.zero;
            newGameObj.transform.localScale = 0.1f * Vector3.one;
            newGameObj.transform.localRotation = Quaternion.identity;
            
            var meshFilter = newGameObj.AddComponent<MeshFilter>();
            meshFilter.sharedMesh = mesh;

            var meshRenderer = newGameObj.AddComponent<MeshRenderer>();
            meshRenderer.sharedMaterial = _material;

            _lookup.Add(mesh, newGameObj);
        }
    }
}