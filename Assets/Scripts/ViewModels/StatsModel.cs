using System.Collections.Specialized;
using System.Linq;
using StlVault.Config;
using StlVault.Util;
using UnityEngine;

namespace StlVault.ViewModels
{
    internal class StatsModel : ModelBase
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
            _model.Current.ValueChanged += CurrentChanged;
            _model.Selection.CollectionChanged += SelectionChanged;
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

        private void CurrentChanged(PreviewInfo obj)
        {
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
            VertexCount.Value = _model.Selection.Sum(pi => pi.GeometryInfo.VertexCount);
            TriangleCount.Value = _model.Selection.Sum(pi => pi.GeometryInfo.TriangleCount);
            Volume.Value = _model.Selection.Sum(pi => pi.GeometryInfo.Volume);
            Width.Value = _model.Selection.Max(pi => Mathf.Abs(pi.GeometryInfo.Size.x));
            Height.Value = _model.Selection.Max(pi => Mathf.Abs(pi.GeometryInfo.Size.y));
            Depth.Value = _model.Selection.Max(pi => Mathf.Abs(pi.GeometryInfo.Size.z));
        }

        private void UpdateFromCurrent()
        {
            if (_model.Current.Value == null)
            {
                Reset();
                return;
            }

            var geometry = _model.Current.Value.GeometryInfo;

            FileName.Value = _model.Current.Value.ItemName;
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