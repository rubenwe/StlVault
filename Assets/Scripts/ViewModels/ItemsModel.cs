using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using JetBrains.Annotations;
using StlVault.Config;
using StlVault.Messages;
using StlVault.Services;
using StlVault.Util;
using StlVault.Util.Collections;
using StlVault.Util.Commands;
using StlVault.Util.Messaging;
using UnityEngine;

namespace StlVault.ViewModels
{
    internal class ItemsModel : 
        IMessageReceiver<SearchChangedMessage>, 
        IMessageReceiver<SelectionChangedMessage>
    {
        [NotNull] private readonly ObservableListWrapper<ItemPreviewModel> _items = new ObservableListWrapper<ItemPreviewModel>();
        [NotNull] private readonly ILibrary _library;
        [NotNull] private readonly IMessageRelay _relay;
        [NotNull] private IEnumerable<string> _currentSearchTags = Enumerable.Empty<string>();
        
        private ItemPreviewModel _lastToggled;

        public IReadOnlyObservableList<ItemPreviewModel> Items => _items;
        public bool SelectRange { private get; set; }

        public ItemsModel([NotNull] ILibrary library, [NotNull] IMessageRelay relay)
        {
            _library = library ?? throw new ArgumentNullException(nameof(library));
            _relay = relay ?? throw new ArgumentNullException(nameof(relay));
            _items.SetSource(new ObservableList<ItemPreviewModel>());
        }

        public void Receive(SearchChangedMessage message)
        {
            if (message.SearchTags.SequenceEqual(_currentSearchTags)) return;
            _currentSearchTags = message.SearchTags ?? Enumerable.Empty<string>();
            
            var data = _library.GetItemPreviewMetadata(message.SearchTags);
            _items.SetSource(data);
            _lastToggled = null;
        }

        public void ToggleAll()
        {
            using (NotifyMassSelection())
            {
                var allSelected = Items.All(item => item.Selected);
                foreach (var previewModel in Items)
                {
                    previewModel.Selected.Value = !allSelected;
                }
            }
        }

        public void ClearSelection()
        {
            using (NotifyMassSelection())
            {
                foreach (var item in _library.GetAllItems())
                {
                    item.Selected.Value = false;
                }
            }
        }

        private IDisposable NotifyMassSelection()
        {
            _relay.Send<MassSelectionStartingMessage>(this);
            return Disposable.FromAction(() => _relay.Send<MassSelectionFinishedMessage>(this));
        }

        public void Receive(SelectionChangedMessage message)
        {
            var sender = message.Sender;
            if (SelectRange)
            {
                // Block subsequent select handler calls ...
                SelectRange = false;

                var lastPos = _lastToggled != null ? -1 : 0;
                var newPos = -1;
                for (var i = 0; i < _items.Count; i++)
                {
                    if (lastPos != -1 && newPos != -1) break;
                    if (_items[i] == sender) newPos = i;
                    if (_items[i] == _lastToggled) lastPos = i;
                }

                if (lastPos != -1)
                {
                    var start = Mathf.Min(lastPos, newPos);
                    var end = Mathf.Max(lastPos, newPos);
                    if (lastPos > newPos) end++;

                    using (NotifyMassSelection())
                    {
                        for (var i = start; i < end; i++)
                        {
                            // ... from here
                            _items[i].Selected.Value = sender.Selected;
                        }
                    }
                }
            }

            _lastToggled = sender;
        }
    }

    
}