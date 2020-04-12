using StlVault.ViewModels;
using TMPro;
using UnityEngine;

#pragma warning disable 0649

namespace StlVault.Views
{
    internal class StatsPanel : ViewBase<IStatsModel>
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
            _fileText.BindTo(ViewModel.FileName);
            _vertexText.BindTo(ViewModel.VertexCount);
            _triangleText.BindTo(ViewModel.TriangleCount);
            _volumeText.BindTo(ViewModel.Volume, "{0:F2} mL");
            _widthText.BindTo(ViewModel.Width, "{0:F1} mm");
            _heightText.BindTo(ViewModel.Height, "{0:F1} mm");
            _depthText.BindTo(ViewModel.Depth, "{0:F1} mm");
        }
    }
}