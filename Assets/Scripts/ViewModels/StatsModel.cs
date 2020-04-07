using System.ComponentModel;
using System.Linq;
using StlVault.Config;
using StlVault.Services;
using StlVault.Util;
using UnityEngine;
using UnityEngine.PlayerLoop;
using NotifyCollectionChangedEventArgs = System.Collections.Specialized.NotifyCollectionChangedEventArgs;

namespace StlVault.ViewModels
{
    internal class StatsModel 
    {
        public BindableProperty<string> FileName { get; } = new BindableProperty<string>();
        public BindableProperty<int> VertexCount { get; } = new BindableProperty<int>();
        public BindableProperty<int> TriangleCount { get; } = new BindableProperty<int>();
        public BindableProperty<float> Width { get; } = new BindableProperty<float>();
        public BindableProperty<float> Height { get; } = new BindableProperty<float>();
        public BindableProperty<float> Depth { get; } = new BindableProperty<float>();
        public BindableProperty<float> Volume { get; } = new BindableProperty<float>();

        private readonly DetailMenuModel _model;
        private SelectionMode Mode => _model.Mode;

        public StatsModel(DetailMenuModel model)
        {
            _model = model;

            _model.Mode.ValueChanged += ModeChanged;
            _model.Current.ValueChanging += CurrentChanging;
            _model.Current.ValueChanged += CurrentChanged;
            _model.Selection.CollectionChanged += SelectionChanged;
        }

        private void CurrentChanging(ItemPreviewModel oldValue)
        {
            if (oldValue == null) return;
            oldValue.GeometryInfo.ValueChanged -= CurrentGeoChanged;
        }

        private void CurrentGeoChanged(GeometryInfo info)
        {
            if(Mode == SelectionMode.Current) UpdateFromCurrent();
        }

        private void ModeChanged(SelectionMode mode)
        {
            if (mode == SelectionMode.Current) UpdateFromCurrent();
            if (mode == SelectionMode.Selection) UpdateFromSelection();
        }

        private void SelectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (Mode == SelectionMode.Selection) UpdateFromSelection();
        }

        private void CurrentChanged(ItemPreviewModel newValue)
        {
            if (newValue != null) newValue.GeometryInfo.ValueChanged += CurrentGeoChanged;
            if (Mode == SelectionMode.Current) UpdateFromCurrent();
        }

        private void UpdateFromSelection()
        {
            if (!_model.Selection.Any())
            {
                Reset();
                return;
            }

            FileName.Value = $"{_model.Selection.Count} Models selected";
            VertexCount.Value = _model.Selection.Sum(pi => pi.GeometryInfo.Value.VertexCount);
            TriangleCount.Value = _model.Selection.Sum(pi => pi.GeometryInfo.Value.TriangleCount);
            Volume.Value = _model.Selection.Sum(pi => pi.GeometryInfo.Value.Volume);
            Width.Value = _model.Selection.Max(pi => Mathf.Abs(pi.GeometryInfo.Value.Size.x));
            Height.Value = _model.Selection.Max(pi => Mathf.Abs(pi.GeometryInfo.Value.Size.y));
            Depth.Value = _model.Selection.Max(pi => Mathf.Abs(pi.GeometryInfo.Value.Size.z));
        }

        private void UpdateFromCurrent()
        {
            if (_model.Current.Value == null)
            {
                Reset();
                return;
            }

            var geometry = _model.Current.Value.GeometryInfo.Value;

            FileName.Value = _model.Current.Value.Name;
            VertexCount.Value = geometry.VertexCount;
            TriangleCount.Value = geometry.TriangleCount;
            Volume.Value = geometry.Volume;
            Width.Value = Mathf.Abs(geometry.Size.x);
            Height.Value = Mathf.Abs(geometry.Size.y);
            Depth.Value = Mathf.Abs(geometry.Size.z);
        }

        private void Reset()
        {
            FileName.Value = string.Empty;
            VertexCount.Value = 0;
            TriangleCount.Value = 0;
            Volume.Value = 0;
            Width.Value = 0;
            Height.Value = 0;
            Depth.Value = 0;
        }
    }
}