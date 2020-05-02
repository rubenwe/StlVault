using System.Collections.Generic;
using System.ComponentModel;
using INotifyCollectionChanged = System.Collections.Specialized.INotifyCollectionChanged;

namespace StlVault.Util.Collections
{
    public interface IReadOnlyObservableCollection<out T> : 
        IReadOnlyCollection<T>,
        INotifyPropertyChanged,
        INotifyCollectionChanged
    {
    }
}