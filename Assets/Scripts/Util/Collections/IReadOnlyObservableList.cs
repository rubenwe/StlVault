using System.Collections.Generic;
using StlVault.Config;

namespace StlVault.Util.Collections
{
    public interface IReadOnlyObservableList<out T> : IReadOnlyObservableCollection<T>, IReadOnlyList<T>
    {
    }
}