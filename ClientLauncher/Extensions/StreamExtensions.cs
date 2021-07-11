using System;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace ClientLauncher.Extensions
{
    public static class StreamExtensions
    {
        public static string Sha256Hash(this Stream stream)
        {
            using var sha256 = SHA256.Create();
            var sha256Hash = sha256.ComputeHash(stream);
            return BitConverter.ToString(sha256Hash).Replace("-", "");
        }

        public static byte[] ReadByteArray(this Stream stream)
        {
            using var memoryStream = new MemoryStream();
            stream.CopyTo(memoryStream);
            return memoryStream.ToArray();
        }
        
        public static async Task<FileInfo> SaveStreamToTempFile(this Stream inStream, string fileName)
        {
            var tempPathRoot = Path.Combine(Path.GetTempPath(), "polusgg-client");
            if (File.Exists(tempPathRoot))
                File.Delete(tempPathRoot);

            if (!Directory.Exists(tempPathRoot))
                Directory.CreateDirectory(tempPathRoot);

            var tempPath = Path.Combine(tempPathRoot, fileName);
            if (File.Exists(tempPath))
                File.Delete(tempPath);

            await using var file = File.OpenWrite(tempPath);
            await inStream.CopyToAsync(file);
            
            return new FileInfo(tempPath);
        }
    }
}