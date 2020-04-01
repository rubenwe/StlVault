using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using StlVault.Config;
using StlVault.Util.Collections;

namespace StlVault.Services
{
    internal sealed class PreviewList : ObservableList<PreviewInfo>, IPreviewList
    {
        [NotNull] private readonly Action<PreviewList> _disposeCallback;
        [NotNull] private readonly Func<IReadOnlyCollection<PreviewInfo>, IReadOnlyCollection<PreviewInfo>> _filter;

        public PreviewList([NotNull] Action<PreviewList> disposeCallback, [NotNull] Func<IReadOnlyCollection<PreviewInfo>, IReadOnlyCollection<PreviewInfo>> filter)
        {
            _disposeCallback = disposeCallback ?? throw new ArgumentNullException(nameof(disposeCallback));
            _filter = filter ?? throw new ArgumentNullException(nameof(filter));
        }

        public void AddFiltered(IReadOnlyCollection<PreviewInfo> previewInfos)
        {
            var filtered = _filter.Invoke(previewInfos);
            AddRange(filtered);
        }

        public void AddFiltered(PreviewInfo previewInfo)
        {
            var array = new[] {previewInfo};
            var filtered = _filter.Invoke(array);
            if (filtered.Count == 1) Add(previewInfo);
        }

        public void Dispose()
        {
            _disposeCallback.Invoke(this);
        }

        public void RemoveRange(HashSet<PreviewInfo> itemsToRemove)
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