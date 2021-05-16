using System;

namespace ClientLauncher.Models
{
    public class PluginDownloadManifest
    {
        public string Version { get; set; } = string.Empty;
        
        public DownloadablePlugin[] Plugins { get; set; } = Array.Empty<DownloadablePlugin>();
    }
}