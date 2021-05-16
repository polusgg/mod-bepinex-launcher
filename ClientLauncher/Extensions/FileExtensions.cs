using System.IO;

namespace ClientLauncher.Extensions
{
    public static class FileExtensions
    {
        public static string MD5Hash(string filePath)
        {
            using var file = File.OpenRead(filePath);
            return file.MD5Hash();
        }
        
        public static bool FileEqualsMD5Hash(string filePath, string md5Hash)
        {
            return MD5Hash(filePath) == md5Hash;
        }
    }
}