using System.Threading;
using System.Threading.Tasks;
using DG.Tweening;
using StlVault.ViewModels;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Button = UnityEngine.UI.Button;

#pragma warning disable 0649

namespace StlVault.Views
{
    internal class ItemView : ViewBase<FilePreviewModel>
    {
        private static readonly Rect PreviewRect = new Rect(Vector2.zero, new Vector2(1024, 1024));
        private static readonly Vector2 Pivot = 0.5f * Vector2.one;

        [SerializeField] private Image _previewImage;
        [SerializeField] private TMP_Text _itemName;
        [SerializeField] private Button _button;
        
        private bool _isLoaded;
        private RectTransform _rect;
        private Camera _mainCam;
        private CancellationTokenSource _source;

        private GameObject _textBanner;
        private Animator _animator;

        private void Start()
        {
            _mainCam = Camera.main;

            _rect = GetComponent<RectTransform>();
            _animator = GetComponent<Animator>();
            
            _previewImage.color = new Color(0, 0, 0, 0);
            _textBanner = _itemName.transform.parent.gameObject;

            _previewImage.gameObject.SetActive(false);
            _textBanner.SetActive(false);
            _animator.enabled = false;
        }

        private static int _number;
        private Texture2D _texture;

        private async void StartLoad()
        {
            _source = new CancellationTokenSource();
            var token = _source.Token;

            try
            {
                var bytes = await ViewModel.LoadPreviewAsync();
                if (bytes == null) return;

                await GetSlot(token);
                if (token.IsCancellationRequested) return;

                _texture = new Texture2D(1024, 1024, TextureFormat.DXT1Crunched, false) {name = ViewModel.Name};
                _texture.LoadImage(bytes);
                _texture.Compress(false);

                _previewImage.sprite = Sprite.Create(_texture, PreviewRect, Pivot, 100, 0, SpriteMeshType.FullRect);
                _previewImage.DOColor(Color.white, 0.2f);

                _isLoaded = true;
            }
            finally
            {
                _source = null;
            }
        }

        private static async Task GetSlot(CancellationToken token)
        {
            while (!ItemViewUpdateQueue.ConsumeSlot())
            {
                if (token.IsCancellationRequested) return;
                
                // ReSharper disable once MethodSupportsCancellation
                await Task.Delay(1);
            }
        }

        private void Update()
        {
            var isVisible = _rect.IsVisibleOn(_mainCam);
            if (isVisible)
            {
                if (!_isLoaded && _source == null) StartLoad();
            }
            else _source?.Cancel();

            _previewImage.gameObject.SetActive(isVisible);
            _textBanner.SetActive(isVisible);
            _animator.enabled = isVisible;
        }

        protected override void OnViewModelBound()
        {
            _itemName.text = ViewModel.Name;
            _button.Bind(ViewModel.SelectCommand);
        }

        private void OnDestroy()
        {
            _previewImage.DOKill();
            _source?.Cancel();
            Destroy(_texture);
        }
    }
}