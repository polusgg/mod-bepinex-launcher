using Newtonsoft.Json;

namespace ClientLauncher.Models
{
    public class DownloadablePlugin
    {
        [JsonProperty("dllName")]
        public string DllName { get; set; } = string.Empty;
        
        [JsonProperty("downloadUrl")]
        public string DownloadUrl { get; set; } = string.Empty;
        
        [JsonProperty("sha256")]
        public string SHA256 { get; set; } = string.Empty;
    }
}