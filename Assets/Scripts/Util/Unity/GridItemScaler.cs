using UnityEngine;
using UnityEngine.UI;

#pragma warning disable 0649

namespace StlVault.Util.Unity
{
    [RequireComponent(typeof(GridLayoutGroup))]
    public class GridItemScaler : MonoBehaviour
    {
        [SerializeField] private Slider _slider;
        [SerializeField] private float _minSize = 100;
        [SerializeField] private float _maxSize = 800;
        [SerializeField] private float _startSize = 400;
        
        private GridLayoutGroup _gridLayout;
        private float _currentVelocity;
    
        void Start()
        {
            _gridLayout = GetComponent<GridLayoutGroup>();
            _slider.minValue = _minSize;
            _slider.maxValue = _maxSize;
            _slider.value = _startSize;
        }
    
        // Update is called once per frame
        private void Update()
        {
            var sliderValue = _slider.value;
            _gridLayout.cellSize = new Vector2(sliderValue, sliderValue + 30);
        }
    }
}
