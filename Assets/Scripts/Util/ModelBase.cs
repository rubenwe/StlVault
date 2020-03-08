using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;

namespace StlVault.Util
{
    public abstract class ModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected bool SetValueAndNotify<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            Debug.Assert(propertyName != null);
            if (Equals(value, field)) return false;

            field = value;
            OnPropertyChanged(propertyName);

            return true;
        }

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}