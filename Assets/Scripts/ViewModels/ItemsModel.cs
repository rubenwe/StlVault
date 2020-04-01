using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using StlVault.AppModel.Messages;
using StlVault.Config;
using StlVault.Services;
using StlVault.Util;
using StlVault.Util.Collections;
using StlVault.Util.Messaging;

namespace StlVault.ViewModels
{
    internal class ItemsModel : ModelBase, IMessageReceiver<SearchChangedMessage>
    {
        [NotNull] private readonly ILibrary _library;
        [NotNull] private readonly IPreviewImageStore _previewImageStore;
        [NotNull] private readonly TrackingCollection<PreviewInfo, FilePreviewModel> _items;
        [NotNull] private IEnumerable<string> _currentSearchTags = Enumerable.Empty<string>();
        
        public IReadOnlyObservableList<FilePreviewModel> Items => _items;

        public ItemsModel([NotNull] ILibrary library, [NotNull] IPreviewImageStore previewImageStore)
        {
            _library = library ?? throw new ArgumentNullException(nameof(library));
            _previewImageStore = previewImageStore ?? throw new ArgumentNullException(nameof(previewImageStore));
            _items = new TrackingCollection<PreviewInfo, FilePreviewModel>(CreateModel);
        }

        public void Receive(SearchChangedMessage message)
        {
            if (message.SearchTags.SequenceEqual(_currentSearchTags)) return;
            _currentSearchTags = message.SearchTags ?? Enumerable.Empty<string>();
            
            var data = _library.GetItemPreviewMetadata(message.SearchTags);
            _items.SetSource(data);
        }

        private FilePreviewModel CreateModel(PreviewInfo info)
        {
            return new FilePreviewModel(_previewImageStore)
            {
                Name = info.ItemName,
                FileHash = info.FileHash,
                InFavorites = false,
                InSelection = false
            };
        }
    }
}