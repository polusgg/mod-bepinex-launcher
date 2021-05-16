namespace ClientLauncher.Models
{
    public class DownloadablePlugin
    {
        public string DllName { get; set; } = string.Empty;
        public string DownloadUrl { get; set; } = string.Empty;
        public string MD5Hash { get; set; } = string.Empty;
    }
}