using Newtonsoft.Json;

namespace ClientLauncher.Models.Accounts
{
    public class LogInRequest
    {
        [JsonProperty("email")] public string Email { get; set; } = "";

        [JsonProperty("password")] public string Password { get; set; } = "";
    }
}