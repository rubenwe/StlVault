using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using StlVault.Util.Stl;
using StlVault.ViewModels;
using UnityEngine;
using UnityEngine.EventSystems;

#pragma warning disable 0649

namespace StlVault.Views
{
    internal class ViewPort : ViewBase<ViewPortModel>, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private Material _material;
        [SerializeField] private Transform _meshParent;
        
        private readonly Dictionary<Mesh, GameObject> _lookup = new Dictionary<Mesh, GameObject>();
        public bool ContainsMousePointer { get; private set; }

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
                    foreach (var (mesh, model) in e.NewItems.OfType<(Mesh, ItemPreviewModel)>())
                    {
                        InstantiateMesh(mesh, model);
                    }
                    
                    break;
                
                case NotifyCollectionChangedAction.Remove:
                    foreach (var(mesh, _) in e.OldItems.OfType<(Mesh, ItemPreviewModel)>())
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
                StlImporter.Destroy(mesh);
                _lookup.Remove(mesh);
            }
        }

        private void InstantiateMesh(Mesh mesh, ItemPreviewModel model)
        {
            if (_lookup.ContainsKey(mesh)) return;
            
            var newGameObj = new GameObject(mesh.name);
            newGameObj.transform.SetParent(_meshParent);
            
            var geoInfo = model.GeometryInfo.Value;
            newGameObj.transform.localPosition = Vector3.zero;
            newGameObj.transform.localScale = 0.1f * geoInfo.Scale;
            newGameObj.transform.localRotation = Quaternion.Euler(geoInfo.Rotation);
            
            var meshFilter = newGameObj.AddComponent<MeshFilter>();
            meshFilter.sharedMesh = mesh;

            var meshRenderer = newGameObj.AddComponent<MeshRenderer>();
            meshRenderer.sharedMaterial = _material;

            _lookup.Add(mesh, newGameObj);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            ContainsMousePointer = true;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            ContainsMousePointer = false;
        }
    }
}