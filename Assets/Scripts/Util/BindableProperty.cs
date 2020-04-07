using System;

namespace StlVault.Util
{
    internal class BindableProperty<T> : IBindableProperty<T>
    {
        private T _value;
        private T _beforeSuppress;
        private bool _active = true;
        public virtual event Action<T> ValueChanged;
        public virtual event Action<T> ValueChanging;

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

                if(_active) ValueChanging?.Invoke(_value);
                _value = value;
                if(_active) ValueChanged?.Invoke(value);
            }
        }

        public static implicit operator T(BindableProperty<T> item) => item.Value;

        public void SuppressUpdates()
        {
            _beforeSuppress = Value;
            _active = false;
        }

        public void ResumeUpdates()
        {
            var current = Value;
            _value = _beforeSuppress;
            _active = true;
            
            Value = current;
        }
    }
}