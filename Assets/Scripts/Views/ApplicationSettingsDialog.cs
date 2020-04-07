using System;
using System.ComponentModel;
using System.Windows.Input;
using JetBrains.Annotations;
using StlVault.Messages;
using StlVault.Util;
using StlVault.Util.Commands;
using StlVault.Util.Messaging;
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

        [SerializeField] private TMP_Text _parallelismLabel;
        [SerializeField] private TMP_Text _logLevelLabel;
        [SerializeField] private TMP_Text _uiScaleLabel;
        [SerializeField] private TMP_Text _previewResolutionLabel;
        [SerializeField] private TMP_Text _previewQualityLabel;
        
        protected override void OnViewModelBound()
        {
            base.OnViewModelBound();
            
            InitSlider(_parallelismSlider, 1, 8);
            InitSlider(_logLevelSlider, 0, 4);
            InitSlider(_uiScaleSlider, 50, 150);
            InitSlider(_previewResolutionSlider, 8, 11);
            InitSlider(_previewQualitySlider, 10, 80);

            _parallelismSlider.Bind(ViewModel.ImportParallelism);
            _uiScaleSlider.Bind(ViewModel.UiScalePercent);
            _logLevelSlider.Bind(ViewModel.LogLevel);
            _previewResolutionSlider.Bind(ViewModel.PreviewResolution);
            _previewQualitySlider.Bind(ViewModel.PreviewJpegQuality);
            
            _parallelismLabel.Bind(ViewModel.ImportParallelism, "{0} Worker");
            _uiScaleLabel.Bind(ViewModel.UiScalePercent, "{0}%");
            _logLevelLabel.Bind(ViewModel.LogLevel);
            _previewResolutionLabel.Bind(ViewModel.PreviewResolution, v => Mathf.Pow(2f, v), "{0:N0}");
            _previewQualityLabel.Bind(ViewModel.PreviewJpegQuality, "{0} %");
        }

        private static void InitSlider(Slider slider, int min, int max)
        {
            slider.wholeNumbers = true;
            slider.minValue = min;
            slider.maxValue = max;
        }
    }
}