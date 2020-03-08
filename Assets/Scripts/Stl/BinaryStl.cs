using System;
using System.Runtime.CompilerServices;
using System.Text;

namespace StlVault.Stl
{
    internal static class BinaryStl
    {
        /// <summary>
        /// Determine whether this file is a binary stl format or not.
        /// </summary>
        public static bool IsBinary(byte[] fileBytes)
        {
            if(fileBytes.Length < 130) return false;
           
            for(var i = 0; i < 80; i++)
            {
                if (fileBytes[i] == 0x0)
                {
                    return true;
                }
            }

            return Encoding.UTF8.GetString(fileBytes, 0, 6) != "solid ";
        }
        
        /// <summary>
        /// UINT8[80] – Header
        ///	UINT32 – Number of triangles
        ///
        ///	foreach triangle <see cref="Facet"/>
        ///		REAL32[3] – Normal vector
        ///		REAL32[3] – Vertex 1
        ///		REAL32[3] – Vertex 2
        ///		REAL32[3] – Vertex 3
        ///		UINT16 – Attribute byte count
        ///	end
        /// </summary>
        public static Facet[] FromBytes(byte[] fileBytes)
        {
            // Discard header
            var facetCount = BitConverter.ToUInt32(fileBytes, 80);
            var facets = new Facet[facetCount];
            
            unsafe
            {
                fixed (byte* fileStart = fileBytes)
                fixed (Facet* destination = facets)
                {
                    var source = fileStart + 84;
                    Unsafe.CopyBlockUnaligned(destination, source, facetCount * 50);
                }
            }
            
            return facets;
        }

        private static readonly byte[] ExportHeader =
        {
            0x0, // this is a binary file
            0x42, 0x4F, 0x4E, 0x45, 0x53, 0x41, 0x57 // BONESAW
        };
        
        public static byte[] ToBytes(Facet[] facets)
        {
            var facetCount = (uint) facets.Length;
            
            var length = 80 + 4 + facetCount * 50;
            var data = new byte[length];
            
            var facetCountBytes = BitConverter.GetBytes(facetCount);
            
            Array.Copy(ExportHeader, data, ExportHeader.Length);
            Array.Copy(facetCountBytes, 0, data, 80, 4);

            unsafe
            {
                fixed (byte* dataStart = data)
                fixed (Facet* source = facets)
                {
                    var destination = dataStart + 84;
                    Unsafe.CopyBlockUnaligned(destination, source, facetCount * 50);
                }
            }

            return data;
        }
    }
}