using System;

namespace StlVault.Util
{
    internal class BindableProperty<T>
    {
        private T _value;
        public event Action<T> ValueChanged;

        public T Value
        {
            get => _value;
            set
            {
                if (TransformValue != null)
                {
                    value = TransformValue(value);
                }
                
                if (Equals(value, _value)) return;
                
                _value = value;
                ValueChanged?.Invoke(value);
            }
        }

        public Func<T, T> TransformValue { private get; set; }
        
        public static implicit operator T(BindableProperty<T> item)
        {
            return item.Value;
        }
    }
}