using UnityEngine;
using UnityEngine.UI;

namespace StlVault.Util.Unity
{
    public class WrapGroup : LayoutGroup
    {
        private static class Axis
        {
            public const int X = 0;
            public const int Y = 1;
        }

        [SerializeField] protected float _spacing = 0;

        /// <summary>
        /// The spacing to use between layout elements in the layout group.
        /// </summary>
        public float Spacing
        {
            get => _spacing;
            set => SetProperty(ref _spacing, value);
        }

        /// <summary>
        /// Called by the layout system. Also see ILayoutElement
        /// </summary>
        public override void CalculateLayoutInputHorizontal()
        {
            base.CalculateLayoutInputHorizontal();
            CalcAlongAxis(Axis.X);
        }

        /// <summary>
        /// Called by the layout system. Also see ILayoutElement
        /// </summary>
        public override void CalculateLayoutInputVertical()
        {
            CalcAlongAxis(Axis.Y);
        }

        private void CalcAlongAxis(int axis)
        {
            var panelWidth = rectTransform.rect.size[Axis.X];
            if (axis == Axis.X)
            {
                SetLayoutInputForAxis(panelWidth, panelWidth, 0, Axis.X);
            }
            else
            {
                float totalHeight = padding.vertical;

                var rowHeight = 0f;
                var rowWidth = 0f;

                foreach (var child in rectChildren)
                {
                    var sizeDelta = child.sizeDelta;
                    var localScale = child.localScale;

                    var width = sizeDelta[Axis.X] * localScale[Axis.X];
                    var height = sizeDelta[Axis.Y] * localScale[Axis.Y];

                    if (rowWidth + width + Spacing > panelWidth - padding.horizontal)
                    {
                        totalHeight += rowHeight + Spacing;
                        rowHeight = 0;
                        rowWidth = 0;
                    }

                    rowWidth += width + Spacing;
                    rowHeight = Mathf.Max(rowHeight, height);
                }

                totalHeight += rowHeight;

                SetLayoutInputForAxis(totalHeight, totalHeight, 0, Axis.Y);
            }
        }

        public override void SetLayoutHorizontal()
        {
            SetAlongAxis(Axis.X);
        }

        public override void SetLayoutVertical()
        {
            SetAlongAxis(Axis.Y);
        }

        private void SetAlongAxis(int axis)
        {
            var pad = padding;

            var panelWidth = rectTransform.rect.size[Axis.X];
            var availableWidth = panelWidth - pad.horizontal;

            float xOffset = pad.left;
            float yOffset = pad.top;
            float rowHeight = 0;

            foreach (var child in rectChildren)
            {
                var sizeDelta = child.sizeDelta;
                var localScale = child.localScale;

                var scaledChildWidth = sizeDelta[Axis.X] * localScale[Axis.X];
                var scaledChildHeight = sizeDelta[Axis.Y] * localScale[Axis.Y];

                if (xOffset + scaledChildWidth + Spacing > availableWidth)
                {
                    xOffset = pad.left;
                    yOffset += rowHeight + Spacing;
                    rowHeight = 0;
                }

                if (axis == Axis.X) SetChildAlongAxis(child, Axis.X, xOffset);
                xOffset += scaledChildWidth + Spacing;

                if (axis == Axis.Y) SetChildAlongAxis(child, Axis.Y, yOffset);
                rowHeight = Mathf.Max(rowHeight, scaledChildHeight);
            }
        }
    }
}