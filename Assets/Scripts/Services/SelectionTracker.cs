using System.Collections.Generic;
using StlVault.Messages;
using StlVault.Util.Messaging;
using StlVault.ViewModels;

namespace StlVault.Services
{
    internal class SelectionTracker :
        IMessageReceiver<MassSelectionStartingMessage>,
        IMessageReceiver<MassSelectionFinishedMessage>,
        IMessageReceiver<SelectionChangedMessage>
    {
        private const string CollectionSelected = "collection: selected";
        
        private readonly HashSet<string> _selected = new HashSet<string>();
        private readonly HashSet<string> _deselected = new HashSet<string>();
        
        private readonly ILibrary _library;
        private bool _runningUpdate;

        public SelectionTracker(ILibrary library)
        {
            _library = library;
        }

        public void Receive(MassSelectionStartingMessage message)
        {
            _runningUpdate = true;
        }

        public void Receive(MassSelectionFinishedMessage message)
        {
            _runningUpdate = false;
            
            _library.AddTag(_selected, CollectionSelected);
            _library.RemoveTag(_deselected, CollectionSelected);
            
            _selected.Clear();
            _deselected.Clear();
        }

        public void Receive(SelectionChangedMessage message)
        {
            var selected = message.Sender.Selected;
            var fileHash = message.Sender.FileHash;

            if (_runningUpdate)
            {
                if (selected) _selected.Add(fileHash);
                else _deselected.Add(fileHash);
            }
            else
            {
                if (selected) _library.AddTag(new[] {fileHash}, CollectionSelected);
                else _library.RemoveTag(new[] {fileHash}, CollectionSelected);
            }
        }
    }
}