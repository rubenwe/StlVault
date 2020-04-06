using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using StlVault.Config;
using StlVault.Messages;
using StlVault.Services;
using StlVault.Util;
using StlVault.Util.Collections;
using StlVault.Util.Messaging;
using UnityEngine;

namespace StlVault.ViewModels
{
    internal class ItemsModel : ModelBase, IMessageReceiver<SearchChangedMessage>
    {
        [NotNull] private readonly ISelectionTracker _tracker;
        [NotNull] private readonly ILibrary _library;
        [NotNull] private readonly IPreviewImageStore _previewImageStore;
        [NotNull] private readonly TrackingCollection<PreviewInfo, FilePreviewModel> _items;
        [NotNull] private IEnumerable<string> _currentSearchTags = Enumerable.Empty<string>();
        private PreviewInfo _lastToggled;

        public IReadOnlyObservableList<FilePreviewModel> Items => _items;
        public bool SelectRange { private get; set; }

        public ItemsModel(
            [NotNull] ILibrary library, 
            [NotNull] ISelectionTracker tracker,
            [NotNull] IPreviewImageStore previewImageStore)
        {
            _library = library ?? throw new ArgumentNullException(nameof(library));
            _tracker = tracker ?? throw new ArgumentNullException(nameof(tracker));
            _previewImageStore = previewImageStore ?? throw new ArgumentNullException(nameof(previewImageStore));
            
            _items = new TrackingCollection<PreviewInfo, FilePreviewModel>(CreateModel);
        }

        public void Receive(SearchChangedMessage message)
        {
            if (message.SearchTags.SequenceEqual(_currentSearchTags)) return;
            _currentSearchTags = message.SearchTags ?? Enumerable.Empty<string>();
            
            var data = _library.GetItemPreviewMetadata(message.SearchTags);
            _items.SetSource(data);
            _lastToggled = null;
        }

        private FilePreviewModel CreateModel(PreviewInfo info)
        {
            void OnSelectedChanged(bool selected)
            {
                if (SelectRange)
                {
                    // Block subsequent select handler calls ...
                    SelectRange = false;
                    
                    var current = _lastToggled;
                    var lastPos = current != null ? -1 : 0;
                    var newPos = -1;
                    for (var i = 0; i < _items.Count; i++)
                    {
                        if (lastPos != -1 && newPos != -1) break;
                        if (_items[i].FileHash == info.FileHash) newPos = i;
                        if (_items[i].FileHash == current?.FileHash) lastPos = i;
                    }

                    if (lastPos != -1)
                    {
                        var start = Mathf.Min(lastPos, newPos);
                        var end = Mathf.Max(lastPos, newPos);
                        if (lastPos > newPos) end++;

                        using (_tracker.EnterMassUpdate())
                        {
                            for (var i = start; i < end; i++)
                            {
                                // ... from here
                                _items[i].Selected.Value = selected;
                            }
                        }
                    }
                }
               
                SetTrackerState(info);

                void SetTrackerState(PreviewInfo previewInfo)
                {
                    if (selected) _tracker.SetSelected(previewInfo);
                    else _tracker.SetDeselected(previewInfo);
                    _lastToggled = previewInfo;
                }
            }
            
            return new FilePreviewModel(_previewImageStore, OnSelectedChanged)
            {
                Name = info.ItemName,
                FileHash = info.FileHash,
                Selected = { Value = _tracker.IsSelected(info)}
            };
        }

        public void SelectAll()
        {
            using (_tracker.EnterMassUpdate())
            {
                foreach (var previewModel in Items)
                {
                    previewModel.Selected.Value = true;
                }
            }
        }
    }
}