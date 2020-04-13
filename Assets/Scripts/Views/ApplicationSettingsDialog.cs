using StlVault.ViewModels;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

#pragma warning disable 0649

namespace StlVault.Views
{
    internal class ApplicationSettingsDialog : DialogView<ApplicationSettingsModel>
    {
        [SerializeField] private Slider _parallelismSlider;
        [SerializeField] private Slider _logLevelSlider;
        [SerializeField] private Slider _uiScaleSlider;
        [SerializeField] private Slider _previewResolutionSlider;
        [SerializeField] private Slider _previewQualitySlider;
        [SerializeField] private Slider _scrollSensitivitySlider;

        [SerializeField] private TMP_Text _parallelismLabel;
        [SerializeField] private TMP_Text _logLevelLabel;
        [SerializeField] private TMP_Text _uiScaleLabel;
        [SerializeField] private TMP_Text _previewResolutionLabel;
        [SerializeField] private TMP_Text _previewQualityLabel;
        [SerializeField] private TMP_Text _scrollSensitivityLabel;
        
        protected override void OnViewModelBound()
        {
            base.OnViewModelBound();
            
            InitSlider(_parallelismSlider, 1, 8);
            InitSlider(_logLevelSlider, 0, 4);
            InitSlider(_uiScaleSlider, 50, 150);
            InitSlider(_previewResolutionSlider, 8, 11);
            InitSlider(_previewQualitySlider, 10, 80);
            InitSlider(_scrollSensitivitySlider, 60, 300);

            _parallelismSlider.BindTo(ViewModel.ImportParallelism);
            _uiScaleSlider.BindTo(ViewModel.UiScalePercent);
            _logLevelSlider.BindTo(ViewModel.LogLevel);
            _previewResolutionSlider.BindTo(ViewModel.PreviewResolution);
            _previewQualitySlider.BindTo(ViewModel.PreviewJpegQuality);
            _scrollSensitivitySlider.BindTo(ViewModel.ScrollSensitivity);
            
            _parallelismLabel.BindTo(ViewModel.ImportParallelism, "{0} Worker");
            _uiScaleLabel.BindTo(ViewModel.UiScalePercent, "{0}%");
            _logLevelLabel.BindTo(ViewModel.LogLevel);
            _previewResolutionLabel.BindTo(ViewModel.PreviewResolution, v => Mathf.Pow(2f, v), "{0:N0}");
            _previewQualityLabel.BindTo(ViewModel.PreviewJpegQuality, "{0} %");
            _scrollSensitivityLabel.BindTo(ViewModel.ScrollSensitivity, v => v/300f * 200, "{0:N0} %");
        }

        private static void InitSlider(Slider slider, int min, int max)
        {
            slider.wholeNumbers = true;
            slider.minValue = min;
            slider.maxValue = max;
        }
    }
}