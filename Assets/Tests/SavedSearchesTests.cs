using System.Linq;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using StlVault.Config;
using StlVault.Messages;
using StlVault.Services;
using StlVault.Util.Commands;
using StlVault.Util.Messaging;
using StlVault.ViewModels;

namespace StlVault.Tests
{
    public class SavedSearchesTests
    {
        private Mock<IMessageRelay> _relay;
        private Mock<IConfigStore> _store;

        private async Task<SavedSearchesModel> InitializeModel()
        {
            _store = new Mock<IConfigStore>();
            _relay = new Mock<IMessageRelay>();

            var config = new SavedSearchesConfigFile
            {
                new SavedSearchConfig {Alias = "Test", Tags = {"Test1", "Test2"}}
            };

            _store.Setup(s => s.LoadAsyncOrDefault<SavedSearchesConfigFile>()).ReturnsAsync(config);
            
            var model = new SavedSearchesModel(_store.Object, _relay.Object);
            await model.InitializeAsync();
            
            return model;
        }

        [Test]
        public void SavedSearchesShouldBeRestoredFromConfig()
        {
            TestUtils.Run(async () =>
            {
                var model = await InitializeModel();

                Assert.IsTrue(model.SavedSearches.Any(search => search.Alias == "Test"));
                Expect.Contains("Test1", model.SavedSearches.First().Tags);
                Expect.Contains("Test2", model.SavedSearches.First().Tags);
            });
        }
        
        [Test]
        public void SelectingSearchShouldSendChangeMessage()
        {
            TestUtils.Run(async () =>
            {
                var model = await InitializeModel();

                Assert.IsTrue(model.SavedSearches.First().SelectCommand.CanExecute());

                model.SavedSearches.First().SelectCommand.Execute();
                _relay.Verify(r => r.Send(model, It.Is<SearchChangedMessage>(msg => msg.SearchTags.Contains("Test1", "Test2"))));
            });
        }
    }
}