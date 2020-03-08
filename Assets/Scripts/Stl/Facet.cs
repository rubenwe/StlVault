using System.Runtime.InteropServices;
using UnityEngine;

namespace StlVault.Stl
{
    /// <summary>
    /// REAL32[3] – Normal vector
    ///	REAL32[3] – Vertex 1
    ///	REAL32[3] – Vertex 2
    ///	REAL32[3] – Vertex 3
    /// UINT16 – Attribute byte count
    /// </summary>
    [StructLayout(LayoutKind.Explicit, Pack = 2)]
    internal struct Facet
    {
        [FieldOffset(00)] public Vector3 normal;
        [FieldOffset(12)] public Vector3 vert_1;
        [FieldOffset(24)] public Vector3 vert_2;
        [FieldOffset(36)] public Vector3 vert_3;
        [FieldOffset(48)] private ushort _flags;
        
        public Facet(Vector3 normal, Vector3 vert1, Vector3 vert2, Vector3 vert3)
        {
            this.normal = normal;
            vert_1 = vert1;
            vert_2 = vert2;
            vert_3 = vert3;
            _flags = 0;
        }

        public override string ToString() => $"{normal}: {vert_1}, {vert_2}, {vert_3}";
    }
}