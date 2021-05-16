using System.IO;

namespace ClientLauncher.Extensions
{
    public static class FileExtensions
    {
        public static string SHA256Hash(string filePath)
        {
            using var file = File.OpenRead(filePath);
            return file.SHA256Hash();
        }
        
        public static bool FileEqualsSHA256Hash(string filePath, string sha256hash)
        {
            return SHA256Hash(filePath) == sha256hash;
        }
    }
}