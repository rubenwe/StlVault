using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace StlVault.Util.Collections
{
    internal class ReadOnlyObservableCollectionWrapper<TBase, TDerived> 
        : ReadOnlyObservableCollection<TDerived>, IReadOnlyObservableCollection<TBase> 
        where TDerived : TBase
    {
        public ReadOnlyObservableCollectionWrapper(ObservableCollection<TDerived> list) : base(list)
        {
        }

        IEnumerator<TBase> IEnumerable<TBase>.GetEnumerator()
        {
            return this.Items.Cast<TBase>().GetEnumerator();
        }
    }

    public class ReadOnlyObservableCollectionWrapper
    {
        public static IReadOnlyObservableCollection<TBase> Create<TBase, TDerived>(IEnumerable<TDerived> items) where TDerived : TBase
        {
            return new ReadOnlyObservableCollectionWrapper<TBase, TDerived>(new ObservableCollection<TDerived>(items));
        }

        public static IReadOnlyObservableCollection<TBase> Create<TBase, TDerived>(ObservableCollection<TDerived> items) where TDerived : TBase
        {
            return new ReadOnlyObservableCollectionWrapper<TBase, TDerived>(items);
        }
    }
}