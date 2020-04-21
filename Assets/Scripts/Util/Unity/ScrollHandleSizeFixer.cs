using UnityEngine;
using UnityEngine.UI;

namespace StlVault.Util.Unity
{
    [RequireComponent(typeof(Scrollbar))]
    public class ScrollHandleSizeFixer : MonoBehaviour
    {
        [SerializeField] private float _minHandleSize = 0.1f;
        private Scrollbar _scrollBar;

        private void Awake()
        {
            _scrollBar = GetComponent<Scrollbar>();
        }

        private void LateUpdate()
        {
            _scrollBar.size = Mathf.Max(_scrollBar.size, _minHandleSize);
        }
    }
}