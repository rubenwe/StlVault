using System;
using System.ComponentModel;
using System.Threading.Tasks;
using JetBrains.Annotations;
using StlVault.Config;
using StlVault.Messages;
using StlVault.Services;
using StlVault.Util.Collections;
using StlVault.Util.Messaging;

namespace StlVault.ViewModels
{
    internal class CollectionsModel : IMessageReceiver<AddCollectionMessage>
    {
        private readonly IConfigStore _store;
        private readonly IMessageRelay _relay;

        public ObservableList<CollectionModel> Collections { get; } = new ObservableList<CollectionModel>();

        public CollectionsModel([NotNull] IConfigStore store, [NotNull] IMessageRelay relay)
        {
            _store = store ?? throw new ArgumentNullException(nameof(store));
            _relay = relay ?? throw new ArgumentNullException(nameof(relay));
        }

        public async Task Initialize()
        {
            var data = await _store.LoadAsyncOrDefault<CollectionsConfigFile>();
            foreach (var config in data)
            {
                Collections.Add(new CollectionModel(config));
            }
        }

        public void Receive(AddCollectionMessage message)
        {
        }
    }
}