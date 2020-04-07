using System;
using System.Collections.Specialized;
using JetBrains.Annotations;

namespace StlVault.Util
{
    internal class DelegateProperty<T> : IBindableProperty<T>
    {
        private readonly BindableProperty<T> _inner;
        private readonly Func<T> _getValue;
        public event Action<T> ValueChanged
        {
            add => _inner.ValueChanged += value;
            remove => _inner.ValueChanged -= value;
        }

        public T Value => _inner.Value;

        public DelegateProperty([NotNull] Func<T> getValue)
        {
            _getValue = getValue ?? throw new ArgumentNullException(nameof(getValue));
            _inner = new BindableProperty<T>(getValue());
        }

        public DelegateProperty<T> UpdateOn([NotNull] INotifyCollectionChanged collection)
        {
            if (collection == null) throw new ArgumentNullException(nameof(collection));
            collection.CollectionChanged += (sender, args) => CheckValue();
            
            return this;
        }
        
        public DelegateProperty<T> UpdateOn<T2>([NotNull] IBindableProperty<T2> property)
        {
            if (property == null) throw new ArgumentNullException(nameof(property));
            property.ValueChanged += v => CheckValue();

            return this;
        }

        private void CheckValue() => _inner.Value = _getValue();
    }
}