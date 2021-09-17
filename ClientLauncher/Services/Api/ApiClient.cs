using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using ClientLauncher.Extensions;
using ClientLauncher.Models;

namespace ClientLauncher.Services.Api
{
    public partial class ApiClient : IDisposable
    {
        private HttpClient _client;
        public ApiClient()
        {
            _client = new HttpClient();
        }

        public async Task<MemoryStream> DownloadFileAsync(string downloadUrl, string? name = null)
        {
            Console.WriteLine(downloadUrl);
            var memoryStream = new MemoryStream();
            await (await _client.GetStreamAsync(downloadUrl)).CopyToAsync(memoryStream);
            return memoryStream;
        }

        public async Task<ModPackageManifest> GetModPackageManifestAsync(string version)
        {
            LoggingService.Log($"{Context.BucketUrl}/{version}/{ModPackageManifest.ManifestFileName}");
            var manifest = await _client.GetFromJsonAsync<ModPackageManifest>($"{Context.BucketUrl}/{version}/{ModPackageManifest.ManifestFileName}");
            if (manifest is null)
                throw new InvalidOperationException($"Download manifest.json was not found for version {version}");
            
            return manifest;
        }

        public void Dispose()
        {
            _client.Dispose();
        }
    }
}