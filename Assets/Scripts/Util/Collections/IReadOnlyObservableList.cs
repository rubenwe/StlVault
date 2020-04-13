using System.Collections.Generic;

namespace StlVault.Util.Collections
{
    public interface IReadOnlyObservableList<out T> : IReadOnlyObservableCollection<T>, IReadOnlyList<T>
    {
    }
}