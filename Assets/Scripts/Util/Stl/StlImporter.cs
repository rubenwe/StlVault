using System;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering;

namespace StlVault.Util.Stl
{
    public static class StlImporter
    {
        public static async Task<(Mesh mesh, string fileHash)> ImportMeshAsync(string path, bool centerVertices = true)
        {
            var fileName = Path.GetFileNameWithoutExtension(path) ?? throw new ArgumentException(nameof(path));

            var fileBytes = await Task.Run(() => File.ReadAllBytes(path)).Timed("Reading file {0}", fileName);
            var isBinary = BinaryStl.IsBinary(fileBytes);

            Task<string> ComputeHash()
            {
                return Task.Run(() => StlImporter.ComputeHash(fileBytes))
                    .Timed("Computing hash of {0}", fileName);
            }

            Facet[] facets;
            Task<string> computeHashTask;
            if (isBinary)
            {
                computeHashTask = Task.Run(ComputeHash);
                facets = await Task.Run(() => BinaryStl.FromBytes(fileBytes))
                    .Timed("Reading binary stl {0}", fileName);
            }
            else
            {
                facets = await Task.Run(() => TextualStl.Import(fileBytes, out _).ToArray())
                    .Timed("Reading textual stl {0}", fileName);

                var convertToBinary = Task.Run(() => fileBytes = BinaryStl.ToBytes(facets))
                    .Timed("Converting {0} to binary stl", fileName);

                computeHashTask = Task.Run(async () =>
                {
                    await convertToBinary;
                    return await ComputeHash();
                });
            }

            var mesh = await CreateMeshFromFacetsAsync(facets, centerVertices, fileName);

            return (mesh, await computeHashTask);
        }

        private static async Task<Mesh> CreateMeshFromFacetsAsync(Facet[] facets, bool centerVertices, string fileName)
        {
            var (vertices, normals, triangles) = await Task.Run(() => BuildMesh(facets))
                .Timed("Building mesh data from {0} imported facets of {1}", facets.Length, fileName);

            var mesh = new Mesh
            {
                name = fileName,
                indexFormat = IndexFormat.UInt32,
                vertices = vertices,
                normals = normals,
                triangles = triangles,
                hideFlags = HideFlags.HideAndDontSave
            };

            if (centerVertices)
            {
                var currentCenter = mesh.bounds.center;
                await Task.Run(() => CenterVertices(vertices, currentCenter))
                    .Timed("Centering vertices of {0}", fileName);

                mesh.vertices = vertices;
                mesh.RecalculateBounds();
            }

            return mesh;
        }

        private static string ComputeHash(byte[] fileBytes)
        {
            using (var sha1 = new SHA1Managed())
            {
                var hash = sha1.ComputeHash(fileBytes);
                return Convert.ToBase64String(hash);
            }
        }

        private static void CenterVertices(Vector3[] vertices, Vector3 correction)
        {
            void MoveVertex(int i) => vertices[i] -= correction;
            Parallel.For(0, vertices.Length, MoveVertex);
        }

        private static (Vector3[] vertices, Vector3[] normals, int[] triangles) BuildMesh(Facet[] facets)
        {
            var meshSize = facets.Length * 3;

            var triangles = new int[meshSize];
            var vertices = new Vector3[meshSize];
            var normals = new Vector3[meshSize];

            void WriteFacet(int currentFacet)
            {
                var i = currentFacet * 3;

                var index0 = i + 0;
                var index1 = i + 1;
                var index2 = i + 2;

                ref var face = ref facets[currentFacet];

                // Invert indices because .stl files are right-handed
                vertices[index0] = new Vector3(-face.vert_1.y, face.vert_1.z, face.vert_1.x);
                vertices[index1] = new Vector3(-face.vert_2.y, face.vert_2.z, face.vert_2.x);
                vertices[index2] = new Vector3(-face.vert_3.y, face.vert_3.z, face.vert_3.x);
                var normal = new Vector3(-face.normal.y, face.normal.z, face.normal.x);

                normals[index0] = normal;
                normals[index1] = normal;
                normals[index2] = normal;

                triangles[index0] = index2;
                triangles[index1] = index1;
                triangles[index2] = index0;
            }

            Parallel.For(0, facets.Length, WriteFacet);

            return (vertices, normals, triangles);
        }
    }
}