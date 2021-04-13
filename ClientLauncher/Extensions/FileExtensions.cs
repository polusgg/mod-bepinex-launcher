using System.IO;

namespace ClientLauncher.Extensions
{
    public static class FileExtensions
    {
        public static bool FileEqualsMD5Hash(string filePath, string md5Hash)
        {
            using var file = File.OpenRead(filePath);
            return file.MD5Hash() == md5Hash;
        }
    }
}