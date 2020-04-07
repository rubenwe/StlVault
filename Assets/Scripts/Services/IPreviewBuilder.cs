using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace StlVault.Services
{
    internal interface IPreviewBuilder
    {
        Task<(byte[] imageData, int resolution)> GetPreviewImageDataAsync(Mesh mesh, Vector3? objRotation);
    }
}