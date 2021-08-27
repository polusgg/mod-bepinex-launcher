using Newtonsoft.Json;

namespace ClientLauncher.Models.Accounts
{
    public class AccountGenericResponse<T>
    {
        [JsonProperty("success")]
        public bool Success { get; set; }
        
        [JsonProperty("data")]
        public T Data { get; set; }
    }
}