using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using StlVault.AppModel.Messages;
using StlVault.Services;
using StlVault.Util;
using StlVault.Util.Collections;
using StlVault.Util.Messaging;

namespace StlVault.ViewModels
{
    internal class ItemsModel : ModelBase, IMessageReceiver<SearchChangedMessage>
    {
        private readonly TrackingCollection<ItemPreviewMetadata, ItemModel> _items 
            = new TrackingCollection<ItemPreviewMetadata, ItemModel>(CreateModel);
        
        private readonly ILibrary _library;
        private IEnumerable<string> _currentSearchTags = Enumerable.Empty<string>();

        public IReadOnlyObservableList<ItemModel> Items => _items;

        public ItemsModel([NotNull] ILibrary library)
        {
            _library = library ?? throw new ArgumentNullException(nameof(library));
        }

        public void Receive(SearchChangedMessage message)
        {
            if (message.SearchTags.SequenceEqual(_currentSearchTags)) return;
            _currentSearchTags = message.SearchTags ?? Enumerable.Empty<string>();
            
            var data = _library.GetItemPreviewMetadata(message.SearchTags);
            _items.SetSource(data);
        }

        private static ItemModel CreateModel(ItemPreviewMetadata metadata)
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