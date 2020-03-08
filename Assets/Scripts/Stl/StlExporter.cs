using System.IO;
using UnityEngine;

namespace StlVault.Stl
{
    public static class StlExporter
    {
        public static void ExportMesh(string path, Mesh mesh)
        {
            var facets = DeconstructMesh(mesh);
            var bytes = BinaryStl.ToBytes(facets);

            File.WriteAllBytes(path, bytes);
        }
        
        private static Facet[] DeconstructMesh(Mesh mesh)
        {
            var vertices = mesh.vertices;
            var normals = mesh.normals;

            var facets = new Facet[vertices.Length / 3];
                
            void WriteFacet(int currentFacet)
            {
                var i = currentFacet * 3;
                
                var index0 = i + 0;
                var index1 = i + 1;
                var index2 = i + 2;
                
                ref var no = ref normals[index0];
                ref var v1 = ref vertices[index0];
                ref var v2 = ref vertices[index1];
                ref var v3 = ref vertices[index2];
                
                // Vector(-y, z, x) => Vector(z, -x, y)
                ref var face = ref facets[currentFacet];

                face.normal = new Vector3(no.z, -no.x, no.y);
                face.vert_1 = new Vector3(v1.z, -v1.x, v1.y); 
                face.vert_2 = new Vector3(v2.z, -v2.x, v2.y); 
                face.vert_3 = new Vector3(v3.z, -v3.x, v3.y);
            }

            for (var i = 0; i < facets.Length; i++)
            {
                WriteFacet(i);
            }            
            
            return facets;
        }
    }
}