// <copyright file="Compress.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace AsyncClientServer.Protocol
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Compression;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// Contains static functions for compressing and decompressing data.
    /// </summary>
    internal static class Compress
    {
        /// <summary>
        /// Compresses the given string via Zip.
        /// </summary>
        /// <param name="str">String that will be zipped.</param>
        /// <returns>Zipped string as bytes.</returns>
        internal static byte[] Zip(in string str)
        {
            var bytes = Encoding.UTF8.GetBytes(str);
            using (var mso = new MemoryStream())
            {
                using (var gs = new GZipStream(mso, CompressionMode.Compress))
                {
                    gs.Write(bytes, 0, bytes.Length);
                }

                return mso.ToArray();
            }
        }

        /// <summary>
        /// Decompresses the given bytes via Zip.
        /// </summary>
        /// <param name="bytes">Bytes that will be unzipped.</param>
        /// <returns>Unzipped bytes as string.</returns>
        internal static string Unzip(in byte[] bytes)
        {
            using (var msi = new MemoryStream(bytes))
            using (var mso = new MemoryStream())
            {
                using (var gs = new GZipStream(msi, CompressionMode.Decompress))
                {
                    gs.CopyTo(mso);
                }

                return Encoding.UTF8.GetString(mso.ToArray());
            }
        }
    }
}
