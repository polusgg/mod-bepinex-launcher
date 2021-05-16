using System;
using Newtonsoft.Json;

namespace ClientLauncher.Models
{
    public class PluginDownloadManifest
    {
        [JsonProperty("version")]
        public string Version { get; set; } = string.Empty;
        
        [JsonProperty("plugins")]
        public DownloadablePlugin[] Plugins { get; set; } = Array.Empty<DownloadablePlugin>();
    }
}