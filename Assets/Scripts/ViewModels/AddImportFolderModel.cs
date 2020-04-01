using System;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using StlVault.Messages;
using StlVault.Util;
using StlVault.Util.Messaging;
using UnityEngine;

namespace StlVault.ViewModels
{
    internal class AddImportFolderModel : DialogModelBase<RequestShowAddImportFolderDialogMessage>
    {
        private readonly IMessageRelay _relay;

        public BindableProperty<string> FolderPath { get; } = new BindableProperty<string>();
        public BindableProperty<bool> ScanSubDirectories { get; } = new BindableProperty<bool>();

        public BindableProperty<string> Tags { get; } = new BindableProperty<string>
            {TransformValue = s => s.ToLowerInvariant()};

        public BindableProperty<bool> RotateOnImport { get; } = new BindableProperty<bool>();
        public BindableProperty<bool> ScaleOnImport { get; } = new BindableProperty<bool>();
        public BindableProperty<Vector3> Rotation { get; } = new BindableProperty<Vector3>();
        public BindableProperty<Vector3> Scale { get; } = new BindableProperty<Vector3>();

        public AddImportFolderModel([NotNull] IMessageRelay relay)
        {
            _relay = relay ?? throw new ArgumentNullException(nameof(relay));

            FolderPath.ValueChanged += path => CanAcceptChanged();
            Rotation.ValueChanged += rotation => RotateOnImport.Value = true;
            Scale.ValueChanged += rotation => ScaleOnImport.Value = true;
        }

        private bool IsImportFolderValid()
        {
            if (string.IsNullOrWhiteSpace(FolderPath)) return false;
            try
            {
                return Directory.Exists(FolderPath);
            }
            catch
            {
                return false;
            }
        }

        protected override bool CanAccept() => IsImportFolderValid();

        protected override void OnAccept()
        {
            _relay.Send(this, new AddImportFolderMessage
            {
                FolderPath = FolderPath,
                ScanSubDirectories = ScanSubDirectories,
                Rotation = Rotation,
                RotateOnImport = RotateOnImport,
                Scale = Scale,
                ScaleOnImport = ScaleOnImport,
                Tags = Tags.Value?
                    .Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries)
                    .Select(tag => tag.Trim())
                    .Where(tag => !string.IsNullOrEmpty(tag))
                    .ToList()
            });
        }

        protected override void Reset()
        {
            FolderPath.Value = string.Empty;
            ScanSubDirectories.Value = true;
            Tags.Value = string.Empty;
            Rotation.Value = Vector3.zero;
            RotateOnImport.Value = false;
            Scale.Value = Vector3.one * 100;
            ScaleOnImport.Value = false;
        }
    }
}