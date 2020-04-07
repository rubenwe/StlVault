using StlVault.ViewModels;
using TMPro;
using UnityEngine;

#pragma warning disable 0649

namespace StlVault.Views
{
    internal class StatsPanel : ViewBase<StatsModel>
    {
        [SerializeField] private TMP_Text _fileText;
        [SerializeField] private TMP_Text _vertexText;
        [SerializeField] private TMP_Text _triangleText;
        [SerializeField] private TMP_Text _volumeText;
        [SerializeField] private TMP_Text _widthText;
        [SerializeField] private TMP_Text _heightText;
        [SerializeField] private TMP_Text _depthText;

        protected override void OnViewModelBound()
        {
            base.OnViewModelBound();

            _fileText.Bind(ViewModel.FileName);
            _vertexText.Bind(ViewModel.VertexCount);
            _triangleText.Bind(ViewModel.TriangleCount);
            _volumeText.Bind(ViewModel.Volume, "{0:F2} mL");
            _widthText.Bind(ViewModel.Width, "{0:F1} mm");
            _heightText.Bind(ViewModel.Height, "{0:F1} mm");
            _depthText.Bind(ViewModel.Depth, "{0:F1} mm");
        }
    }
}