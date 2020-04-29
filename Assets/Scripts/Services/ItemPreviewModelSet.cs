using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using JetBrains.Annotations;
using StlVault.Config;
using StlVault.Util;
using StlVault.Util.Collections;
using StlVault.Util.FileSystem;
using StlVault.Util.Messaging;
using StlVault.ViewModels;
using UnityEngine;
using NotifyCollectionChangedEventHandler = System.Collections.Specialized.NotifyCollectionChangedEventHandler;

namespace StlVault.Services
{
    internal class ItemPreviewModelSet : IReadOnlyObservableList<ItemPreviewModel>
    {
        private readonly Dictionary<string, ItemPreviewModel> _modelsByHash = new Dictionary<string, ItemPreviewModel>();
        private readonly ObservableList<ItemPreviewModel> _models = new ObservableList<ItemPreviewModel>();
        private readonly Dictionary<string, IFileSource> _sources = new Dictionary<string, IFileSource>();
        private readonly IPreviewImageStore _store;
        private readonly IMessageRelay _relay;

        public ItemPreviewModelSet([NotNull] IPreviewImageStore previewImageStore, [NotNull] IMessageRelay relay)
        {
            _store = previewImageStore ?? throw new ArgumentNullException(nameof(previewImageStore));
            _relay = relay ?? throw new ArgumentNullException(nameof(relay));
        }

        public void Initialize(MetaData metaData)
        {
            var newModels = metaData.Select(info => new ItemPreviewModel(_store, _relay, info));
            _models.AddRange(newModels);
            foreach (var model in _models)
            {
                _modelsByHash[model.FileHash] = model;
            }
        }

        public bool TryGetValue(string hash, out ItemPreviewModel model) => _modelsByHash.TryGetValue(hash, out model);

        public IReadOnlyList<ItemPreviewModel> Matching(IReadOnlyList<string> tags)
        {
            return this.Where(model => tags.All(model.Tags.Contains)).ToList();
        }

        public Dictionary<string, ImportedFileInfo> GetKnownFiles(IFileSource source)
        {
            _sources[source.DisplayName] = source;
            
            return _models
                .SelectMany(model => model.Sources.Where(info => info.SourceId == source.DisplayName))
                .ToDictionary(info => info.FilePath);
        }

        public ItemPreviewModel AddOrUpdate(IFileSource source, IFileInfo file, PreviewInfo info, GeometryInfo geoInfo)
        {
            var hash = info.FileHash;
            if (!_modelsByHash.TryGetValue(hash, out var model))
            {
                model = _modelsByHash[hash] = new ItemPreviewModel(_store, _relay, info);
                model.GeometryInfo.Value = geoInfo;
                
                _models.Add(model);
            }
            else
            {
                model.Tags.AddRange(info.Tags);
            }

            _sources[source.DisplayName] = source;
            model.AddSourceFile(source.DisplayName, file);

            return model;
        }

        public ItemPreviewModel RemoveOrUpdate(IFileSource source, string relativePath)
        {
            _sources[source.DisplayName] = source;
            bool IsFile(ImportedFileInfo info)
            {
                return info.FilePath == relativePath && info.SourceId == source.DisplayName;
            }

            foreach (var model in _models)
            {
                var file = model.Sources.FirstOrDefault(IsFile);
                if (file == null) continue;

                model.Sources.Remove(file);
                if (model.Sources.Count == 0)
                {
                    _models.Remove(model);
                    _modelsByHash.Remove(model.FileHash);
                    model.Selected.Value = false;
                }

                return model;
            }

            return null;
        }

        public MetaData GetMetaData()
        {
            var metaData = new MetaData();

            metaData.AddRange(_models.Select(model =>
                new PreviewInfo
                {
                    ItemName = model.Name,
                    Sources = model.Sources.ToList(),
                    Tags = model.Tags.ToHashSet(),
                    FileHash = model.FileHash,
                    Resolution = model.PreviewResolution,
                    
                    Volume = model.GeometryInfo.Value.Volume,
                    Size = model.GeometryInfo.Value.Size,
                    VertexCount = model.GeometryInfo.Value.VertexCount,
                    
                    Rotation = model.GeometryInfo.Value.Rotation == Vector3.zero 
                        ? (ConfigVector3?) null 
                        : model.GeometryInfo.Value.Rotation,
                    Scale = model.GeometryInfo.Value.Scale == Vector3.one 
                        ? (ConfigVector3?) null 
                        : model.GeometryInfo.Value.Scale
                }));

            return metaData;
        }

        public ItemPreviewModel this[int index] => _models[index];
        public IEnumerator<ItemPreviewModel> GetEnumerator() => _models.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        public int Count => _models.Count;

        public event PropertyChangedEventHandler PropertyChanged
        {
            add => _models.PropertyChanged += value;
            remove => _models.PropertyChanged -= value;
        }

        public event NotifyCollectionChangedEventHandler CollectionChanged
        {
            add => _models.CollectionChanged += value;
            remove => _models.CollectionChanged -= value;
        }

        public (IFileSource source, string filePath) TryGetFileSource(ItemPreviewModel model)
        {
            foreach (var info in model.Sources.OrderByDescending(s => s.LastChange))
            {
                if (_sources.TryGetValue(info.SourceId, out var source))
                {
                    return (source, info.FilePath);
                }
            }

            return (null, null);
        }
    }
}