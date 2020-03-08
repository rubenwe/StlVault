using System;
using System.Linq;
using JetBrains.Annotations;
using StlVault.AppModel.Messages;
using StlVault.Util;
using StlVault.Util.Collections;
using StlVault.Util.Messaging;

namespace StlVault.AppModel.ViewModels
{
    internal class ItemsModel : ModelBase, IMessageReceiver<SearchChangedMessage>
    {
        private readonly ObservableList<ItemModel> _items = new ObservableList<ItemModel>();
        private readonly ILibrary _library;
        
        public IReadOnlyObservableList<ItemModel> Items => _items;
        
        public ItemsModel([NotNull] ILibrary library)
        {
            _library = library ?? throw new ArgumentNullException(nameof(library));
        }
        
        public void Receive(SearchChangedMessage message)
        {
            var data = _library.GetItemPreviewMetadata(message.SearchTags);
            using (_items.EnterMassUpdate())
            {
                _items.Clear();
                _items.AddRange(data.Select(CreateModel));
            }
        }

        private ItemModel CreateModel(ItemPreviewMetadata metadata)
        {
            return new ItemModel
            {
                Name = metadata.ItemName,
                PreviewImagePath = metadata.PreviewImagePath,
                InFavorites = false,
                InSelection = false
            };
        }
    }
}

