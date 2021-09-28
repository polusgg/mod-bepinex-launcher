using Newtonsoft.Json;

namespace ClientLauncher.Models {
    public class LauncherClientInfo {
        public const string LauncherInfoName = "launcher.info.json"; 
        [JsonProperty("version")]
        public int LauncherVersion { get; set; }
    }
}