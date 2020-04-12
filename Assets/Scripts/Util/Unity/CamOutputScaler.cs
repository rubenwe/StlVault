using UnityEngine;

#pragma warning disable 0649

namespace StlVault.Util.Unity
{
    [RequireComponent(typeof(Camera))]
    [ExecuteInEditMode]
    internal class CamOutputScaler : MonoBehaviour
    {
        [SerializeField] private RectTransform _rectTransform;
        private Camera _camera;

        private void Awake()
        {
            _camera = GetComponent<Camera>();
        }

        
        private void Update()
        {
            var rect = _rectTransform.rect.size * _rectTransform.lossyScale;
            var width = 1f - rect.x / Screen.width;
            var height = 1f - rect.y / Screen.height;
            
            _camera.rect = new Rect(width, height, 1f, 1f);
        }
    }
}