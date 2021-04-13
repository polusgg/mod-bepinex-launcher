using System;
using System.IO;
using System.Security.Cryptography;

namespace ClientLauncher.Extensions
{
    public static class StreamExtensions
    {
        public static string MD5Hash(this Stream stream)
        {
            using var md5 = MD5.Create();
            var md5hash = md5.ComputeHash(stream);
            return BitConverter.ToString(md5hash).Replace("-", "");
        }

        public static byte[] ReadByteArray(this Stream stream)
        {
            using var memoryStream = new MemoryStream();
            stream.CopyTo(memoryStream);
            return memoryStream.ToArray();
        }
    }
}