using System;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;

namespace StlVault.Util.Unity
{
    [RequireComponent(typeof(RectTransform))]
    public class AdaptiveHeight : MonoBehaviour
    {
        [SerializeField] private RectTransform _heightDriver;
        [SerializeField] private float _baseHeight;
        private RectTransform _self;
        private float _previousHeight;
        private TweenerCore<Vector2, Vector2, VectorOptions> _tween;


        private void Start()
        {
            _self = GetComponent<RectTransform>();
        }

        private void Update()
        {
            if (Math.Abs(_heightDriver.sizeDelta.y - _previousHeight) > 0.5f)
            {
                var target = new Vector2(_self.sizeDelta.x, _heightDriver.sizeDelta.y + _baseHeight);
                
                _tween?.Kill();
                _tween = _self.DOSizeDelta(target, 0.2f);
                
                _previousHeight = _heightDriver.sizeDelta.y;
            }
        }
    }
}