using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using StlVault.Util.Logging;
using ILogger = StlVault.Util.Logging.ILogger;

namespace StlVault.Util.Stl
{
    internal static class BinaryStl
    {
        private static readonly ILogger Logger = UnityLogger.Instance;
        
        /// <summary>
        /// Determine whether this file is a binary stl format or not.
        /// </summary>
        public static bool IsBinary(byte[] fileBytes)
        {
            // Minimum length for header + one facet
            if (fileBytes.Length < 130) return false;

            for (var i = 0; i < 80; i++)
            {
                // Null bytes should be used for empty header bytes
                if (fileBytes[i] == 0x0)
                {
                    return true;
                }
            }

            for (var i = 80; i < 130; i++)
            {
                // Chars outside of ASCII range are likely for binary files
                if (fileBytes[i] > 126) return true;
            }

            // According to spec this should no be the case for binary files!
            // But nobody seems to care - so this is a last ditch effort..
            return Encoding.ASCII.GetString(fileBytes, 0, 6) != "solid ";
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
            var dataFacetCount = BitConverter.ToUInt32(fileBytes, 80);
            var calculatedCount = (uint) ((fileBytes.LongLength - 84) / 50);

            if (dataFacetCount > calculatedCount)
                throw new InvalidDataException("The facet count specified in the STL file is too big for the file!");

            if (dataFacetCount != calculatedCount)
                Logger.Warn("Calculated facet count and the one in STL file don't match up!");

            var usedCount = Math.Min(dataFacetCount, calculatedCount);
            var facets = new Facet[usedCount];
            
            unsafe
            {
                fixed (byte* fileStart = fileBytes)
                fixed (Facet* destination = facets)
                {
                    var source = fileStart + 84;
                    Unsafe.CopyBlockUnaligned(destination, source, usedCount * 50);
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