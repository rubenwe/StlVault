using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using INotifyCollectionChanged = System.Collections.Specialized.INotifyCollectionChanged;

namespace StlVault.Util.Collections
{
    public interface IReadOnlyObservableList<out T> : IReadOnlyList<T>, INotifyPropertyChanged, INotifyCollectionChanged
    {
    }
}