using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using StlVault.Util.Collections;

namespace StlVault.Services
{
    internal sealed class PreviewList : ObservableList<ItemPreviewMetadata>, IPreviewList
    {
        [NotNull] private readonly Action<PreviewList> _disposeCallback;
        [NotNull] private readonly Func<IReadOnlyCollection<ItemPreviewMetadata>, IReadOnlyCollection<ItemPreviewMetadata>> _filter;

        public PreviewList([NotNull] Action<PreviewList> disposeCallback, [NotNull] Func<IReadOnlyCollection<ItemPreviewMetadata>, IReadOnlyCollection<ItemPreviewMetadata>> filter)
        {
            _disposeCallback = disposeCallback ?? throw new ArgumentNullException(nameof(disposeCallback));
            _filter = filter ?? throw new ArgumentNullException(nameof(filter));
        }

        public void AddFiltered(IReadOnlyCollection<ItemPreviewMetadata> metadata)
        {
            var filtered = _filter.Invoke(metadata);
            AddRange(filtered);
        }

        public void Dispose()
        {
            _disposeCallback.Invoke(this);
        }

        public void RemoveRange(HashSet<ItemPreviewMetadata> itemsToRemove)
        {
            using (EnterMassUpdate())
            {
                for (var i = Count - 1; i >= 0; i--)
                {
                    if (itemsToRemove.Contains(this[i])) RemoveAt(i);
                }
            }
        }
    }
}