using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace StlVault.Util
{
    public static class EnumerableExtensions
    {
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        public static Dictionary<K, V> SafeToDictionary<K, V>(this IEnumerable<V> items, Func<V, K> keySelector)
        {
            var dict = new Dictionary<K, V>();
            foreach (var item in items)
            {
                var key = keySelector(item);
                dict[key] = item;
            }

            return dict;
        }
    }
}