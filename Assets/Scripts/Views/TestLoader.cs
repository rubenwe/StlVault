using System.IO;
using StlVault.Util.Stl;
using UnityEngine;

namespace StlVault.Views
{
    public class TestLoader : MonoBehaviour
    {
        [SerializeField] private Material _material;
        
        public async void Start()
        {
            var display = new GameObject("test_obj");
            var newTransform = display.transform;
            newTransform.SetParent(transform);
            newTransform.localScale = 0.1f * Vector3.one;

            var filter = display.AddComponent<MeshFilter>();
            var renderer = display.AddComponent<MeshRenderer>();
            renderer.material = _material;
            
            var filePath = @"C:\Users\weitu\Desktop\artisan guild\Amazons\Amazon Archers\Amazon_Archer_A (repaired).stl";
            var fileName = Path.GetFileName(filePath);
            var bytes = File.ReadAllBytes(filePath);
            var (mesh, _) = await StlImporter.ImportMeshAsync(fileName, bytes, false, false);
            filter.sharedMesh = mesh;
        }
    }
}