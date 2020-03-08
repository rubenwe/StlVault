using System;
using System.Collections.Generic;

namespace StlVault.Util
{
    public static class HashSetExtensions
    {
        public static HashSet<T> ToHashSet<T>(this IEnumerable<T> source)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            return new HashSet<T>(source);
        }
    }
}