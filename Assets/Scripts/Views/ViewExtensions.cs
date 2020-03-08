using System;
using System.Globalization;
using System.Windows.Input;
using StlVault.Util;
using StlVault.Util.Commands;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace StlVault.Views
{
    internal static class ViewExtensions
    {
        public static void Bind(this Button button, ICommand command)
        {
            command.CanExecuteChanged += (sender, args) => button.interactable = command.CanExecute();
            button.interactable = command.CanExecute();
            
            button.onClick.AddListener(command.Execute);
        }
        
        public static void Bind<T>(this TMP_InputField inputField, BindableProperty<T> property)
        {
            property.ValueChanged += OnPropertyChanged;
            inputField.onValueChanged.AddListener(OnDisplayValueChanged);
            OnPropertyChanged(property);
            
            void OnPropertyChanged(T newValue) => inputField.text = newValue?.ToString();
            void OnDisplayValueChanged(string newValue) => property.Value = (T) Convert.ChangeType(newValue, typeof(T));
        }
        
        public static void Bind<T>(this TMP_Text text, BindableProperty<T> property)
        {
            property.ValueChanged += OnPropertyChanged;
            OnPropertyChanged(property);
            
            void OnPropertyChanged(T newValue) => text.text = newValue?.ToString();
        }
        
        public static void Bind(
            this (TMP_InputField x, TMP_InputField y, TMP_InputField z) fields, 
            BindableProperty<Vector3> property, 
            string format = "F0")
        {
            var (x, y, z) = fields;
            
            property.ValueChanged += OnPropertyChanged;
            
            x.onValueChanged.AddListener(OnDisplayValueChanged);
            y.onValueChanged.AddListener(OnDisplayValueChanged);
            z.onValueChanged.AddListener(OnDisplayValueChanged);
            
            OnPropertyChanged(property);

            void OnPropertyChanged(Vector3 newValue)
            {
                x.text = newValue.x.ToString(format, CultureInfo.InvariantCulture);
                y.text = newValue.y.ToString(format, CultureInfo.InvariantCulture);
                z.text = newValue.z.ToString(format, CultureInfo.InvariantCulture);
            }
            
            void OnDisplayValueChanged(string newValue)
            {
                property.Value = new Vector3(
                    float.Parse(x.text, CultureInfo.InvariantCulture), 
                    float.Parse(y.text, CultureInfo.InvariantCulture), 
                    float.Parse(z.text, CultureInfo.InvariantCulture));
            }
        }
        
        public static void Bind(this Toggle toggle, BindableProperty<bool> property)
        {
            property.ValueChanged += OnPropertyChanged;
            toggle.onValueChanged.AddListener(OnDisplayValueChanged);
            OnPropertyChanged(property);
            
            void OnPropertyChanged(bool newValue) => toggle.isOn = newValue;
            void OnDisplayValueChanged(bool newValue) => property.Value = newValue;
        }
    }
}