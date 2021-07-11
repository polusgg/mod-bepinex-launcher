using System;
using Newtonsoft.Json;

namespace ClientLauncher.Models
{
    public class ModPackageManifest
    {
        public const string ManifestFileName = "modpackage.manifest.json"; 
        
        [JsonProperty("version")]
        public uint Version { get; set; }
        
        [JsonProperty("files")]
        public ModPackageFile[] Files { get; set; } = Array.Empty<ModPackageFile>();
    }
}