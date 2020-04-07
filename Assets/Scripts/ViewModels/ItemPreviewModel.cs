using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using StlVault.Config;
using StlVault.Services;
using StlVault.Util;
using StlVault.Util.Collections;
using StlVault.Util.FileSystem;
using StlVault.Util.Messaging;

namespace StlVault.ViewModels
{
    internal sealed class ItemPreviewModel
    {
        private readonly IPreviewImageStore _previewImageStore;

        public string Name { get; }
        public string FileHash { get; }

        public event Action PreviewChanged;
        public BindableProperty<GeometryInfo> GeometryInfo { get; } = new BindableProperty<GeometryInfo>();
        public ObservableSet<string> Tags { get; } = new ObservableSet<string>();
        public BindableProperty<bool> Selected { get; } = new BindableProperty<bool>();
        public List<ImportedFileInfo> Sources { get; }
        public int PreviewResolution { get; set; }

        public Task<byte[]> LoadPreviewAsync()
        {
            return _previewImageStore.LoadPreviewAsync(FileHash);
        }
        
        public ItemPreviewModel(
            [NotNull] IPreviewImageStore previewImageStore, 
            [NotNull] IMessageRelay relay, 
            [NotNull] PreviewInfo previewInfo)
        {
          _previewImageStore = previewImageStore ?? throw new ArgumentNullException(nameof(previewImageStore));
          if (previewInfo is null) throw new ArgumentNullException(nameof(previewInfo));
          if(relay is null) throw new ArgumentNullException(nameof(relay));

          Name = previewInfo.ItemName;
          FileHash = previewInfo.FileHash;
          GeometryInfo.Value = previewInfo.GeometryInfo;
          Tags.AddRange(previewInfo.Tags);
          Sources = previewInfo.Sources?.ToList() ?? new List<ImportedFileInfo>();
          PreviewResolution = previewInfo.Resolution;

          Selected.ValueChanged += value => relay.Send(this, new SelectionChangedMessage{Sender = this});
        }

        public void OnPreviewChanged() => PreviewChanged?.Invoke();

        public void AddSourceFile(string sourceId, IFileInfo file)
        {
            var sourceInfo = Sources.FirstOrDefault(info => info.SourceId == sourceId && info.FilePath == file.Path);
            
            if (sourceInfo == null) sourceInfo = new ImportedFileInfo {FilePath = file.Path, SourceId = sourceId};
            else Sources.Remove(sourceInfo);

            sourceInfo.LastChange = file.LastChange;
            Sources.Add(sourceInfo);
        }
    }
}