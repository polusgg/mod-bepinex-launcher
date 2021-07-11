using Newtonsoft.Json;

namespace ClientLauncher.Models
{
    public class ModPackageFile
    {
        [JsonProperty("installPath")]
        public string InstallPath { get; set; } = string.Empty;
        
        [JsonProperty("downloadUrl")]
        public string DownloadUrl { get; set; } = string.Empty;
        
        [JsonProperty("sha256")]
        public string SHA256 { get; set; } = string.Empty;
    }
}