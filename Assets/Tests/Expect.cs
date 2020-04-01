using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace StlVault.Tests
{
    public static class Expect
    {
        public static void Contains<T>(T expected, IEnumerable<T> items, string message = null)
        {
            Assert.Contains(expected, items.ToList(), message);
        }

        public static bool Contains<T>(this IEnumerable<T> items, params T[] expected)
        {
            return expected.All(items.Contains);
        }
    }
}