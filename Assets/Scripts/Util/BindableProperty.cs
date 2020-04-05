using System;
using StlVault.Config;
using StlVault.Util.Collections;

namespace StlVault.Util
{
    internal class BindableProperty<T> : IBindableProperty<T>
    {
        private T _value;
        public virtual event Action<T> ValueChanged;

        public Func<T, T> TransformValue { private get; set; }

        public BindableProperty()
        {
        }

        public BindableProperty(T value)
        {
            _value = value;
        }

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

        public static implicit operator T(BindableProperty<T> item) => item.Value;
    }
}