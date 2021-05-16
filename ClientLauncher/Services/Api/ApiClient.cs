using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using ClientLauncher.Extensions;
using ClientLauncher.Models;

namespace ClientLauncher.Services.Api
{
    public class ApiClient : IDisposable
    {
        private HttpClient _client;
        public ApiClient()
        {
            _client = new HttpClient();
        }

        public async Task<string> DownloadFileAsync(string downloadUrl, string? name = null)
        {
            var stream = await _client.GetStreamAsync(downloadUrl);
            return await stream.SaveStreamToTempFile(name ?? new Guid().ToString());
        }

        public async Task<PluginDownloadManifest?> GetPluginDownloadManifestAsync(string version)
        {
            return await _client.GetFromJsonAsync<PluginDownloadManifest>($"{Context.BucketUrl}/{version}/manifest.json");
        }

        public void Dispose()
        {
            _client.Dispose();
        }
    }
}