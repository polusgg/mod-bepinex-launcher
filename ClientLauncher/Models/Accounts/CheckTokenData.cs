using Newtonsoft.Json;

namespace ClientLauncher.Models.Accounts
{
    public class CheckTokenData
    {
        [JsonProperty("client_id")]
        public string ClientId { get; set; } = "";
        
        [JsonProperty("client_token")]
        public string ClientToken { get; set; } = "";
        
        [JsonProperty("display_name")]
        public string DisplayName { get; set; } = "";
        
        [JsonProperty("banned_until")]
        public string BannedUntil { get; set; } = "";

        [JsonProperty("perks")]
        public string[] Perks { get; set; } = { };
    }
}