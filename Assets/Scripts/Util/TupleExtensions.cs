using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace StlVault.Util
{
    public static class TupleExtensions
    {
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        public static void Deconstruct<K, V>(this KeyValuePair<K, V> pair, out K key, out V value)
        {
            key = pair.Key;
            value = pair.Value;
        }
    }
}