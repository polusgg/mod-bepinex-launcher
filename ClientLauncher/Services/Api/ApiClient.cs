using System;
using System.IO;
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

        public async Task<FileInfo> DownloadFileAsync(string downloadUrl, string? name = null)
        {
            Console.WriteLine(downloadUrl);
            var stream = await _client.GetStreamAsync(downloadUrl);
            return await stream.SaveStreamToTempFile(name ?? Guid.NewGuid().ToString());
        }

        public async Task<ModPackageManifest> GetModPackageManifestAsync(string version)
        {
            Console.WriteLine($"{Context.BucketUrl}/{version}/{ModPackageManifest.ManifestFileName}");
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