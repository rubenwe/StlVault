using UnityEngine;

#pragma warning disable 0649

namespace StlVault.Views
{
    internal class PreviewCam : MonoBehaviour
    {
        [SerializeField] private Camera _camera;
        [SerializeField] private MeshFilter _meshFilter;
        [SerializeField] private int _previewResolution = 1024;
        [SerializeField] private int _scaleFactor = 8;
        
        private RenderTexture _renderTexture;
        private Texture2D _texture2D;

        private void Start()
        {
            _camera.aspect = 1.0f;
            _renderTexture = new RenderTexture(_previewResolution, _previewResolution, 24);
            _camera.targetTexture = _renderTexture;
            _texture2D = new Texture2D(_previewResolution,_previewResolution, TextureFormat.RGB24, false);
        }

        public byte[] GetSnapshot(Mesh mesh, Vector3? objRotation, int quality)
        {
            if (objRotation != null)
            {
                _meshFilter.transform.rotation = Quaternion.Euler(objRotation.Value);
            }
         
            _meshFilter.transform.localScale =  Vector3.one * (_scaleFactor / Max(mesh.bounds.size));
            _meshFilter.mesh = mesh;
            _camera.Render();

            RenderTexture.active = _renderTexture;
            _texture2D.ReadPixels(new Rect(0, 0, _previewResolution, _previewResolution), 0, 0);
            RenderTexture.active = null;

            return _texture2D.EncodeToJPG(quality);
        }
        
        private static float Max(Vector3 size)
        {
            return Max(size.x, size.y, size.z);
        }

        private static float Max(params float[] values)
        {
            var max = float.MinValue;
            for (var i = 0; i < values.Length; i++)
            {
                ref var f = ref values[i];
                if (f > max)
                {
                    max = f;
                }
            }

            return max;
        }
    }
}