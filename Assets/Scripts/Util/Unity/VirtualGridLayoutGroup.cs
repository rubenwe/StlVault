using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using StlVault.Util.Collections;
using StlVault.Views;
using UnityEngine;
using UnityEngine.UI;

namespace StlVault.Util.Unity
{
    public abstract class VirtualGridLayoutGroup<TModel, TChildView, TChildModel> : LayoutGroup, IView<TModel>
        where TChildView : MonoBehaviour, IView<TChildModel>
    {
        private static class Axis
        {
            public const int X = 0;
            public const int Y = 1;
        }
        
        [SerializeField] private Slider _slider;
        [SerializeField] private float _minSize = 200;
        [SerializeField] private float _maxSize = 1000;
        [SerializeField] private Vector2 _childRatioOffset = new Vector2(0, 30);
        [SerializeField] private TChildView _childViewPrefab;
        [SerializeField] private RectTransform _viewPortRect;
        [SerializeField] private float _spacing;

        private Vector2 _childSize = new Vector2(200, 230);
        private bool _reposition;
        
        private readonly Dictionary<int, TChildView> _views = new Dictionary<int, TChildView>();
       
        public TModel ViewModel { get; private set; }
        protected abstract IReadOnlyObservableList<TChildModel> ChildModels { get;  }
        public void BindTo(TModel viewModel)
        {
            ViewModel = viewModel;
            ChildModels.OnMainThread().CollectionChanged += ChildModelsChanged;
        }

        private void ChildModelsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action != NotifyCollectionChangedAction.Add)
            {
                foreach (var view in _views.Values) Destroy(view.gameObject);
                _views.Clear();
            }

            LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
        }

        private int ColumnCount => 
            Mathf.Max(1,Mathf.FloorToInt((rectTransform.rect.width - padding.horizontal + _spacing + 0.001f) / (_childSize.x + _spacing)));

        protected override void Start()
        {
            base.Start();
            _slider.minValue = _minSize;
            _slider.maxValue = _maxSize;
            _slider.onValueChanged.AddListener(OnSliderValueChanged);
            _slider.value = (_maxSize - _minSize) / 2 + _minSize;
        }

        private void OnSliderValueChanged(float value)
        {
            _childSize = new Vector2(value, value) + _childRatioOffset;
            LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
        }

        public override void CalculateLayoutInputHorizontal()
        {
            var width = padding.horizontal + (_childSize.x + _spacing) * ColumnCount - _spacing;
            
            SetLayoutInputForAxis(width, width, -1, Axis.X);
        }

        public override void CalculateLayoutInputVertical()
        {
            if (ChildModels == null)
            {
                SetLayoutInputForAxis(0, 0, -1, Axis.Y);
                return;
            }

            var minRows = Mathf.CeilToInt(ChildModels.Count / (float) ColumnCount);
            var minSpace = padding.vertical + (_childSize.y + _spacing) * minRows - _spacing;
            
            SetLayoutInputForAxis(minSpace, minSpace, -1, Axis.Y);
        }

        public override void SetLayoutHorizontal() => _reposition = true;
        public override void SetLayoutVertical() => _reposition = true;
        
        protected virtual void Update()
        {
            if (ChildModels == null || !_reposition) return;

            var columnCount = ColumnCount;
            var viewPortHeight = _viewPortRect.rect.height;
            var yStart = rectTransform.anchoredPosition.y;
            var yEnd = yStart + viewPortHeight;

            var itemSizeX = _childSize.x + _spacing;
            var itemSizeY = _childSize.y + _spacing;

            var offset = padding;
            var paddingTop = offset.top;
            var paddingLeft = offset.left;
            var paddingRight = offset.right;
            
            var firstRow = Mathf.Max(0, Mathf.FloorToInt((yStart - paddingTop) / itemSizeY));
            var lastRow = Mathf.Max(Mathf.CeilToInt((yEnd - paddingTop) / itemSizeY));

            var firstIdx = columnCount * firstRow;
            var lastIdx = Mathf.Min(columnCount * lastRow, ChildModels.Count);
            var usedSpace = (columnCount * itemSizeX + paddingLeft + paddingRight - _spacing);
            var emptySpace = rectTransform.rect.width - usedSpace;
            var realPaddingLeft = paddingLeft + emptySpace / 2;
            
            for (var i = firstIdx; i < lastIdx; i++)
            {
                if (!_views.TryGetValue(i, out var view))
                {
                    view = Instantiate(_childViewPrefab, transform);
                    view.BindTo(ChildModels[i]);
                    _views.Add(i, view);
                }

                if (_reposition)
                {
                    var rect = view.GetComponent<RectTransform>();
                    rect.sizeDelta = _childSize;
                
                    var viewPosY = itemSizeY * (int) (i / ColumnCount) + paddingTop;
                    var viewPosX = itemSizeX * (int) (i % ColumnCount) + realPaddingLeft;
                    view.transform.localPosition = new Vector2(viewPosX, - viewPosY);
                }
            }

            var dropped = _views.Keys.Where(idx => idx < firstIdx || idx >= lastIdx).ToList();
            foreach (var idx in dropped)
            {
                if (_views.TryGetValue(idx, out var view))
                {
                    Destroy(view.gameObject);
                    _views.Remove(idx);
                }
            }
        }
    }
}