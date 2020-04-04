using System.Threading.Tasks;
using StlVault.Config;
using StlVault.Util.Unity;
using UnityEngine;

namespace StlVault.Services
{
    internal class GeometryInfo
    {
        public Vector3 Rotation { get; set; }

        public int TriangleCount { get; set; }

        public int VertexCount { get; set; }

        public float Volume { get; set; }

        public Vector3 Size { get; set; }

        public Vector3 Scale { get; set; }

        public static Task<GeometryInfo> FromMeshAsync(Mesh mesh, FileSourceConfig config)
        {
            var tcs = new TaskCompletionSource<GeometryInfo>();
            GuiCallbackQueue.Enqueue(() =>
            {
                var rotation = config.Rotation != null
                    ? (Vector3) config.Rotation.Value
                    : Vector3.zero;

                var scale = config.Scale != null
                    ? (Vector3) config.Scale.Value
                    : Vector3.one;

                var rotated = Quaternion.Euler(rotation) * mesh.bounds.size;
                var size = new Vector3(rotated.x * scale.x, rotated.y * scale.y, rotated.z * scale.z);

                var vertices = mesh.vertices;

                var volume = 0f;
                for (var i = 0; i < vertices.Length; i += 3)
                {
                    var a = vertices[i + 0];
                    var b = vertices[i + 1];
                    var c = vertices[i + 2];

                    volume += Vector3.Dot(Vector3.Cross(a, b), c) / 6f;
                }

                volume = Mathf.Abs(volume * scale.x * scale.y * scale.z) / 1000f;

                var info =  new GeometryInfo
                {
                    Rotation = rotation,
                    Scale = scale,
                    Size = size,
                    Volume = volume,
                    VertexCount = vertices.Length,
                    TriangleCount = vertices.Length / 3
                };

                tcs.SetResult(info);
            });

            return tcs.Task;
        }
    }
}