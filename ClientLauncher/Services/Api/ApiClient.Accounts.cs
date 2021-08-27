using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using ClientLauncher.Models;
using ClientLauncher.Models.Accounts;
using ClientLauncher.Models.Cosmetics;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Steamworks;

namespace ClientLauncher.Services.Api
{
    public partial class ApiClient
    {
        private static JsonSerializerSettings _snakeCaseSettings = new()
        {
            ContractResolver = new DefaultContractResolver
            {
                NamingStrategy = new SnakeCaseNamingStrategy()
            }
        };
            
        internal async Task<AccountGenericResponse<LoginResponse>> LogIn(string email, string password)
        {
            var request = new HttpRequestMessage
            {
                RequestUri = new Uri($"{Context.AccountServerUrl}/api/v1/auth/token"),
                Method = HttpMethod.Post,
                Content = new StringContent(JsonConvert.SerializeObject(new LogInRequest
                {
                    Email = email,
                    Password = password
                }), Encoding.UTF8, "application/json")
            };

            var response = await _client.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                var result = JsonConvert.DeserializeObject<AccountGenericResponse<LoginResponse>>(
                    await response.Content.ReadAsStringAsync(),
                    _snakeCaseSettings
                );
                if (result is not null)
                    return result;
            }

            return null;
        }

        internal async Task<AccountGenericResponse<CheckTokenData>> CheckToken(string clientId, string clientToken)
        {
            var request = new HttpRequestMessage
            {
                RequestUri = new Uri($"{Context.AccountServerUrl}/api/v1/auth/check"),
                Method = HttpMethod.Post
            };
            request.Headers.Add("Client-ID", clientId);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", clientToken);

            var response = await _client.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                var result = JsonConvert.DeserializeObject<AccountGenericResponse<CheckTokenData>>(
                    await response.Content.ReadAsStringAsync(),
                    _snakeCaseSettings
                );
                if (result is not null)
                    return result;
            }

            return null;
        }
    }
}