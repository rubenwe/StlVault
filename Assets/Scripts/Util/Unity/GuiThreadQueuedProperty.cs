using System;

namespace StlVault.Util.Unity
{
    internal class GuiThreadQueuedProperty<T> : BindableProperty<T>
    {
        private event Action<T> GuiValueChanged;

        public override event Action<T> ValueChanged
        {
            add => GuiValueChanged += value;
            remove => GuiValueChanged -= value;
        }

        public GuiThreadQueuedProperty(BindableProperty<T> property)
        {
            property.ValueChanged += value => GuiCallbackQueue.Enqueue(() => GuiValueChanged?.Invoke(value));
        }
    }
}