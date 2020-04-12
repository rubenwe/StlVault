using System;
using System.Globalization;
using System.Windows.Input;
using StlVault.Util;
using StlVault.Util.Collections;
using StlVault.Util.Commands;
using StlVault.Util.Logging;
using StlVault.Util.Unity;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace StlVault.Views
{
    internal static class ViewExtensions
    {
        public static void BindTo(this Button button, ICommand command)
        {
            command.CanExecuteChanged += (sender, args) => button.interactable = command.CanExecute();
            button.interactable = command.CanExecute();

            button.onClick.AddListener(command.Execute);
        }

        public static void BindTo<T>(this SimpleButton button, ICommand command, T param)
        {
            void OnChange(object sender, EventArgs args) => button.Enabled.Value = command.CanExecute(param);
            void OnButtonOnClicked() => command.Execute(param);
            
            command.CanExecuteChanged += OnChange;
            button.Enabled.Value = command.CanExecute(param);

            button.Clicked += OnButtonOnClicked;
        }
        
        public static void BindTo(this SimpleButton button, ICommand command)
        {
            void OnChange(object sender, EventArgs args) => button.Enabled.Value = command.CanExecute();
            void OnButtonOnClicked() => command.Execute();
            
            command.CanExecuteChanged += OnChange;
            button.Enabled.Value = command.CanExecute();

            button.Clicked += OnButtonOnClicked;
        }

        public static void BindTo<T>(this TMP_InputField inputField, BindableProperty<T> property)
        {
            property.ValueChanged += OnPropertyChanged;
            inputField.onValueChanged.AddListener(OnDisplayValueChanged);
            OnPropertyChanged(property);

            void OnPropertyChanged(T newValue) => inputField.text = newValue?.ToString();
            void OnDisplayValueChanged(string newValue) => property.Value = (T) Convert.ChangeType(newValue, typeof(T));
        }

        public static void BindTo<T>(this TMP_Text text, BindableProperty<T> property, string formatString = null)
        {
            property.ValueChanged += OnPropertyChanged;
            OnPropertyChanged(property);

            void OnPropertyChanged(T newValue) => text.text = formatString != null 
                ? string.Format(formatString, newValue)
                : newValue?.ToString();
        }
        
        public static void BindTo<T>(this TMP_Text text, BindableProperty<T> property, Func<T, object> compute, string formatString = null)
        {
            property.ValueChanged += OnPropertyChanged;
            OnPropertyChanged(property);

            void OnPropertyChanged(T newValue) => text.text = formatString != null 
                ? string.Format(formatString, compute(newValue))
                : compute(newValue)?.ToString();
        }

        public static void BindTo(
            this (TMP_InputField x, TMP_InputField y, TMP_InputField z) fields,
            BindableProperty<Vector3> property,
            string formatString = "F0")
        {
            var (x, y, z) = fields;

            property.ValueChanged += OnPropertyChanged;

            x.onValueChanged.AddListener(OnDisplayValueChanged);
            y.onValueChanged.AddListener(OnDisplayValueChanged);
            z.onValueChanged.AddListener(OnDisplayValueChanged);

            OnPropertyChanged(property);

            void OnPropertyChanged(Vector3 newValue)
            {
                x.text = newValue.x.ToString(formatString, CultureInfo.InvariantCulture);
                y.text = newValue.y.ToString(formatString, CultureInfo.InvariantCulture);
                z.text = newValue.z.ToString(formatString, CultureInfo.InvariantCulture);
            }

            void OnDisplayValueChanged(string newValue)
            {
                property.Value = new Vector3(
                    float.Parse(x.text, CultureInfo.InvariantCulture),
                    float.Parse(y.text, CultureInfo.InvariantCulture),
                    float.Parse(z.text, CultureInfo.InvariantCulture));
            }
        }

        public static void BindTo(this Toggle toggle, BindableProperty<bool> property)
        {
            property.ValueChanged += OnPropertyChanged;
            toggle.onValueChanged.AddListener(OnDisplayValueChanged);
            OnPropertyChanged(property);

            void OnPropertyChanged(bool newValue) => toggle.isOn = newValue;
            void OnDisplayValueChanged(bool newValue) => property.Value = newValue;
        }

        public static void BindTo(this Slider slider, BindableProperty<ushort> property)
        {
            property.ValueChanged += OnPropertyChanged;
            slider.onValueChanged.AddListener(OnDisplayValueChanged);
            OnPropertyChanged(property);

            void OnPropertyChanged(ushort newValue) => slider.value = newValue;
            void OnDisplayValueChanged(float newValue) => property.Value = (ushort) newValue;
        }
        
        public static void BindTo(this Slider slider, BindableProperty<LogLevel> property)
        {
            property.ValueChanged += OnPropertyChanged;
            slider.onValueChanged.AddListener(OnDisplayValueChanged);
            OnPropertyChanged(property);

            void OnPropertyChanged(LogLevel newValue) => slider.value = (int) newValue;
            void OnDisplayValueChanged(float newValue) => property.Value = (LogLevel) (int) newValue;
        }

        public static BindableProperty<T> OnMainThread<T>(this BindableProperty<T> property)
        {
            return new GuiThreadQueuedProperty<T>(property);
        }

        public static IReadOnlyObservableList<T> OnMainThread<T>(this IReadOnlyObservableList<T> list)
        {
            return new GuiThreadQueuedReadOnlyObservableList<T>(list);
        }
    }
}