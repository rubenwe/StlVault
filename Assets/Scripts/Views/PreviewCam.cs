using System.Threading.Tasks;
using StlVault.Services;
using StlVault.Util;
using StlVault.Util.Unity;
using UnityEngine;

#pragma warning disable 0649

namespace StlVault.Views
{
    internal class PreviewCam : MonoBehaviour, IPreviewBuilder
    {
        public BindableProperty<int> PreviewResolution { get; } = new BindableProperty<int>();
        public int Quality { get; set; }

        [SerializeField] private Camera _camera;
        [SerializeField] private MeshFilter _meshFilter;
        [SerializeField] private int _scaleFactor = 8;

        private RenderTexture _renderTexture;
        private Texture2D _texture2D;

        private void Awake()
        {
            PreviewResolution.ValueChanged += SetupTextures;
        }

        private void Start()
        {
            _camera.aspect = 1.0f;
        }

        private void SetupTextures(int res)
        {
            if (_renderTexture != null)
            {
                _camera.targetTexture = null;
                Destroy(_renderTexture);
                Destroy(_texture2D);
            }
            
            _renderTexture = new RenderTexture(res, res, 24);
            _camera.targetTexture = _renderTexture;
            _texture2D = new Texture2D(res, res, TextureFormat.RGB24, false);
        }

        private byte[] GetSnapshot(Mesh mesh, Vector3? objRotation)
        {
            if (objRotation != null)
            {
                _meshFilter.transform.rotation = Quaternion.Euler(objRotation.Value);
            }

            _meshFilter.transform.localScale = Vector3.one * (_scaleFactor / Max(mesh.bounds.size));
            _meshFilter.mesh = mesh;
            _camera.Render();

            RenderTexture.active = _renderTexture;
            _texture2D.ReadPixels(new Rect(0, 0, _texture2D.width, _texture2D.height), 0, 0);
            RenderTexture.active = null;

            return _texture2D.EncodeToJPG(Quality);
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

        public Task<(byte[] imageData, int resolution)> GetPreviewImageDataAsync(Mesh mesh, Vector3? objRotation)
        {
            var tcs = new TaskCompletionSource<(byte[], int)>(TaskCreationOptions.RunContinuationsAsynchronously);
            GuiCallbackQueue.Enqueue(() =>
            {
                var data = GetSnapshot(mesh, objRotation);
                tcs.SetResult((data, PreviewResolution.Value));
            });

            return tcs.Task;
        }
    }
}