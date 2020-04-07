using System.Collections.Generic;
using System.Threading.Tasks;
using StlVault.Config;
using StlVault.Util;
using StlVault.Util.Collections;
using StlVault.ViewModels;
using UnityEngine;


namespace StlVault.Services
{
    internal interface ILibrary : ITagIndex
    {
        IPreviewList GetItemPreviewMetadata(IReadOnlyList<string> filters);

        void AddTag(IEnumerable<string> hashes, string tag);
        void RemoveTag(IEnumerable<string> hashes, string tag);
        Task RotateAsync(ItemPreviewModel previewModel, Vector3 newRotation);
        IEnumerable<ItemPreviewModel> GetAllItems();
    }
}