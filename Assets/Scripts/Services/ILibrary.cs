using System.Collections.Generic;
using System.Threading.Tasks;
using StlVault.Util.Tags;
using StlVault.ViewModels;
using UnityEngine;


namespace StlVault.Services
{
    internal interface ILibrary
    {
        IPreviewList GetItemPreviewMetadata(IReadOnlyList<string> filters);

        void AddTag(IEnumerable<string> hashes, string tag);
        void RemoveTag(IEnumerable<string> hashes, string tag);
        Task RotateAsync(ItemPreviewModel previewModel, Vector3 newRotation);
        IEnumerable<ItemPreviewModel> GetAllItems();
        Task<Mesh> GetMeshAsync(ItemPreviewModel model);
        bool TryGetLocalPath(ItemPreviewModel model, out string localPath);
        Vector3 GetImportRotation(ItemPreviewModel previewModel);
        IReadOnlyList<TagSearchResult> GetRecommendations(IEnumerable<string> currentFilters, string search);
    }
}