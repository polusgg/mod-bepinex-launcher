using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using ClientLauncher.Models;
using ClientLauncher.Models.Cosmetics;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Steamworks;

namespace ClientLauncher.Services.Api
{
    public partial class ApiClient
    {
        private static readonly JsonSerializerSettings _jsonSettings = new()
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        };
        
        private SavedAuthModel GetAuthModel()
        {
            var model = JsonConvert.DeserializeObject<SavedAuthModel>(
                Encoding.UTF8.GetString(Convert.FromBase64String(File.ReadAllText(Path.Combine(Context.ModdedAmongUsLocation, "api.txt"))))
            );

            if (model is null)
                throw new InvalidOperationException(
                    "Could not find api.txt from Among Us, are you logged in on the client?");
            return model;
        }
        
        public async Task<CosmeticBundle[]> GetAllBundles()
        {
            var bundles = await _client.GetFromJsonAsync<CosmeticBundle[]>($"{Context.CosmeticsUrl}/v1/bundle");
            if (bundles is null)
                throw new WebException($"Could not fetch cosmetic bundles.");
            return bundles;
        }
        
        public async Task<CosmeticBundle> GetBundle(string bundleId)
        {
            var bundle = await _client.GetFromJsonAsync<GenericResponse<CosmeticBundle>>($"{Context.CosmeticsUrl}/v1/bundle/{bundleId}");
            if (bundle is null)
                throw new WebException($"Could not fetch cosmetic bundle id '{bundleId}'.");
            return bundle.Data;
        }
        
        public async Task<CosmeticItem> GetItem(string itemId)
        {
            var item = await _client.GetFromJsonAsync<GenericResponse<CosmeticItem>>($"{Context.CosmeticsUrl}/v1/item/{itemId}");
            if (item is null)
                throw new WebException($"Could not fetch cosmetic item id '{itemId}'.");
            return item.Data;
        }
        
        public async Task<PurchaseInitResponse> InitMicroTransaction(string bundleId)
        {
            if (!SteamClient.IsValid)
                throw new InvalidOperationException(
                    $"Cannot InitMicroTransaction for bundle '{bundleId}', Steam is not initialized");
            
            var model = GetAuthModel();

            var request = new HttpRequestMessage
            {
                RequestUri = new Uri($"{Context.CosmeticsUrl}/v1/bundle/{bundleId}/purchase/steam"),
                Method = HttpMethod.Post,
                Content =  new StringContent(JsonConvert.SerializeObject(new
                {
                    UserId = SteamClient.SteamId.Value
                }, _jsonSettings), Encoding.UTF8, "application/json")
            };
            request.Headers.TryAddWithoutValidation("Authorization", $"{model.ClientToken}:{model.ClientIdString}");

            var response = await (await _client.SendAsync(request)).Content.ReadFromJsonAsync<PurchaseInitResponse>();
            if (response is null)
                throw new WebException("Trying to init a bundle purchase transaction returned null");
            
            return response;
        }

        public async Task FinalizeTransaction(string purchaseId)
        {
            var model = GetAuthModel();

            var request = new HttpRequestMessage
            {
                RequestUri = new Uri($"{Context.CosmeticsUrl}/v1/purchases/{purchaseId}/finalize"),
                Method = HttpMethod.Post,
            };
            request.Headers.TryAddWithoutValidation("Authorization", $"{model.ClientToken}:{model.ClientIdString}");

            var response = await _client.SendAsync(request);
        }
    }
}