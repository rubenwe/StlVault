using System;
using System.ComponentModel;
using System.Windows.Input;
using JetBrains.Annotations;
using StlVault.Messages;
using StlVault.Util;
using StlVault.Util.Commands;
using StlVault.Util.Messaging;
using StlVault.ViewModels;
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

        protected override void OnViewModelBound()
        {
            base.OnViewModelBound();
            
            InitSlider(_parallelismSlider, 1, 8);
            InitSlider(_logLevelSlider, 0, 4);
            InitSlider(_uiScaleSlider, 50, 150);

            _parallelismSlider.Bind(ViewModel.ImportParallelism);
            _uiScaleSlider.Bind(ViewModel.UiScalePercent);
            _logLevelSlider.Bind(ViewModel.LogLevel);
        }

        private static void InitSlider(Slider slider, int min, int max)
        {
            slider.wholeNumbers = true;
            slider.minValue = min;
            slider.maxValue = max;
        }
    }
}