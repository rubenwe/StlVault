using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using StlVault.Util.Logging;
using StlVault.Util.Stl;
using StlVault.Views;
using UnityEngine;
using ILogger = StlVault.Util.Logging.ILogger;

namespace StlVault.Services
{
    [RequireComponent(typeof(PreviewCam))]
    internal class PreviewBuilder : MonoBehaviour, IPreviewBuilder
    {
        private static readonly ILogger Logger = UnityLogger.Instance;
        private CancellationTokenSource _import;
        private PreviewCam _previewCam;

        private void Awake()
        {
            _previewCam = GetComponent<PreviewCam>();
        }

        private async void RebuildPreviews(IReadOnlyList<ItemPreviewMetadata> itemMetaData)
        {
            await Task.Delay(1);

            _import = new CancellationTokenSource();
            var token = _import.Token;

            var sw = Stopwatch.StartNew();
            foreach (var item in itemMetaData)
            {
                if (token.IsCancellationRequested) return;
                if (File.Exists(item.PreviewImagePath)) continue;

                await BuildPreview(item);
            }

            Logger.Info($"Finished import of {itemMetaData.Count} items in {sw.Elapsed.TotalSeconds}s.");
        }

        private void OnDestroy()
        {
            _import?.Cancel();
        }

        private async Task BuildPreview(ItemPreviewMetadata obj)
        {
            var sw = Stopwatch.StartNew();
            var sb = new StringBuilder();

            var (mesh, hash) = await StlImporter.ImportMeshAsync(obj.StlFilePath);

            sb.AppendLine($"Imported {obj.ItemName} - Took {sw.ElapsedMilliseconds}ms.");
            sb.AppendLine($"Vertices: {mesh.vertexCount}, Hash: {hash}");

            var snapshot = _previewCam.GetSnapshot(mesh, obj.Rotation, 80);
            File.WriteAllBytes(obj.PreviewImagePath, snapshot);

            Destroy(mesh);

            Logger.Debug(sb.ToString());
        }
    }
}