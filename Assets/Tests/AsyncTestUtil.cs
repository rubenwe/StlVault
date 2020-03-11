using System;
using System.Threading.Tasks;

namespace StlVault.Tests
{
    public static class AsyncTestUtil
    {
        public static void Run(Func<Task> action)
        {
            action().GetAwaiter().GetResult();
        }
    }
}