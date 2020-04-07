using System;
using System.Collections.Generic;
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
        private readonly DetailMenuModel _detailMenu;
        private readonly ILibrary _library;
        private SelectionMode Mode => _detailMenu.Mode;

        public TagEditorModel([NotNull] DetailMenuModel detailMenu, [NotNull] ILibrary library)
            : base(library)
        {
            _detailMenu = detailMenu ?? throw new ArgumentNullException(nameof(detailMenu));
            _library = library ?? throw new ArgumentNullException(nameof(library));
            
            _detailMenu.Mode.ValueChanged += OnModeChanged;
            _detailMenu.Selection.CollectionChanged += (s, a) => SelectionChanged();
            _detailMenu.Current.ValueChanged += CurrentChanged;
        }

        private void OnModeChanged(SelectionMode mode)
        {
            if (mode == Current) CurrentChanged(_detailMenu.Current);
            if (mode == Selection) SelectionChanged();
        }

        private void CurrentChanged(ItemPreviewModel current)
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
            
            var selectionTags = _detailMenu.Selection.Select(pi => Filter(pi.Tags)).ToList();
            if (!selectionTags.Any())
            {
                Tags.Clear();
                return;
            }

            var shared = selectionTags.First();
            var complete = shared.ToHashSet();
            foreach (var tags in selectionTags.Skip(1))
            {
                shared.IntersectWith(tags.ToList());
                complete.UnionWith(tags.ToList());
            }

            var targetTags = complete
                .Select(text => new TagModel(text, RemoveTag) {IsPartial = !shared.Contains(text)})
                .OrderBy(model => model.IsPartial)
                .ThenBy(model => model.Text);
            
            Tags.ChangeTo(targetTags);
        }

        private HashSet<string> Filter(IReadOnlyCollection<string> tags)
        {
            var filtered = tags
                .Where(t => !t.StartsWith("folder:"));

            return new HashSet<string>(filtered);
        }

        private IEnumerable<string> Hashes => Mode == Current
            ? new[] {_detailMenu.Current.Value.FileHash}
            : _detailMenu.Selection.Select(pi => pi.FileHash);

        protected override bool CanPinCurrentInput() => _detailMenu.AnythingSelected.Value;
        protected override void OnTagAdded(string tag) => _library.AddTag(Hashes, tag);
        protected override void OnTagRemoved(string tag) => _library.RemoveTag(Hashes, tag);
    }
}