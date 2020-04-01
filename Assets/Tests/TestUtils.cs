using System;
using System.Threading.Tasks;
using Moq;
using StlVault.AppModel;
using StlVault.Services;

namespace StlVault.Tests
{
    internal static class TestUtils
    {
        public static void Run(Func<Task> action)
        {
            action().GetAwaiter().GetResult();
        }

        public static Mock<IConfigStore> CreateStore<T>(T config) where T : class, new()
        {
            var store = new Mock<IConfigStore>();
            store.Setup(s => s.LoadAsyncOrDefault<T>()).ReturnsAsync(config);
            
            return store;
        }
    }
}