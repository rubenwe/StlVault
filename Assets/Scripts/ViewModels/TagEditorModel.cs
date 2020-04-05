using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using JetBrains.Annotations;
using StlVault.Config;
using StlVault.Services;
using StlVault.Util;
using static StlVault.ViewModels.SelectionMode;

namespace StlVault.ViewModels
{
    internal class TagEditorModel : TagInputModelBase
    {
        public IBindableProperty<bool> InputEnabled { get; }

        private readonly DetailMenuModel _detailMenu;
        private readonly ILibrary _library;
        private SelectionMode Mode => _detailMenu.Mode;

        public TagEditorModel([NotNull] DetailMenuModel detailMenu, [NotNull] ILibrary library)
            : base(library)
        {
            _detailMenu = detailMenu ?? throw new ArgumentNullException(nameof(detailMenu));
            _library = library ?? throw new ArgumentNullException(nameof(library));
            
            _detailMenu.Selection.CollectionChanged += (s, a) => SelectionChanged();
            _detailMenu.Current.ValueChanged += CurrentChanged;
            
            InputEnabled = new DelegateProperty<bool>(AnythingSelected)
                .UpdateOn(_detailMenu.Current)
                .UpdateOn(_detailMenu.Selection);
        }

        private void CurrentChanged(PreviewInfo current)
        {
            if (Mode != Current) return;
            if (current == null)
            {
                Tags.Clear();
                return;
            }

            var filtered = Filter(current.Tags);
            
            Tags.ChangeTo(filtered.Select(text => new TagModel(text, RemoveTag)));
        }

        private void SelectionChanged()
        {
            if (Mode != Selection) return;
            
            var sharedTags = _detailMenu.Selection.Select(pi => Filter(pi.Tags)).ToList();
            if (!sharedTags.Any())
            {
                Tags.Clear();
                return;
            }

            var first = sharedTags.First();
            foreach (var tags in sharedTags.Skip(1))
            {
                first.IntersectWith(tags);
            }
            
            Tags.ChangeTo(first.Select(text => new TagModel(text, RemoveTag)));
        }

        private HashSet<string> Filter(HashSet<string> tags)
        {
            var filtered = tags
                .Where(t => !t.StartsWith("folder:"));

            return new HashSet<string>(filtered);
        }

        private bool AnythingSelected() => Mode == Current
            ? _detailMenu.Current.Value != null
            : _detailMenu.Selection.Any();
        
        private IEnumerable<string> Hashes => Mode == Current
            ? new[] {_detailMenu.Current.Value.FileHash}
            : _detailMenu.Selection.Select(pi => pi.FileHash);

        protected override void OnTagAdded(string tag) => _library.AddTagAsync(Hashes, tag);
        protected override void OnTagRemoved(string tag) => _library.RemoveTagAsync(Hashes, tag);
    }
}