using System;

namespace StlVault.Util
{
    internal interface IBindableProperty<out T>
    {
        event Action<T> ValueChanged;
        T Value { get; }
    }
}