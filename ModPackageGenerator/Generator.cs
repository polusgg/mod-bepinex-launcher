using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using ClientLauncher.Extensions;
using ClientLauncher.Models;
using Newtonsoft.Json;

namespace ModPackageGenerator
{
    public class Generator
    {
        private readonly List<ModPackageFile> _files = new();

        public string PackageFolder { get; }
        public string BucketUrl { get; }
        public string AmongUsVersion { get; }

        public Generator(string packageFolder, string bucketUrl, string amongUsVersion)
        {
            PackageFolder = Path.GetFullPath(packageFolder);
            AmongUsVersion = amongUsVersion;

            BucketUrl = bucketUrl.TrimEnd('/');
        }

        public void AddFile(string installPath, string downloadUrl, string sha256Hash)
        {
            _files.Add(new ModPackageFile
            {
                InstallPath = installPath,
                DownloadUrl = downloadUrl,
                SHA256 = sha256Hash
            });
        }
        
        public async ValueTask GenerateAsync()
        {
            foreach (var filePath in Directory.EnumerateFiles(PackageFolder, "*", SearchOption.AllDirectories))
            {
                if (filePath.Contains(ModPackageManifest.ManifestFileName))
                    continue;

                var rebasedPath = filePath.Replace(PackageFolder, "").Replace('\\', '/').TrimStart('/');
                AddFile(rebasedPath, $"{BucketUrl}/{AmongUsVersion}/{rebasedPath}", FileExtensions.Sha256Hash(filePath));
            }

            var manifest = new ModPackageManifest
            {
                Version = await GetUploadedManifestVersion() + 1,
                Files = _files.ToArray()
            };
            
            await File.WriteAllTextAsync(Path.Combine(PackageFolder, ModPackageManifest.ManifestFileName), JsonConvert.SerializeObject(manifest, Formatting.Indented));
        }

        private async Task<uint> GetUploadedManifestVersion()
        {
            using var client = new HttpClient();

            uint version = 0;
            try
            {
                var manifest = await client.GetFromJsonAsync<ModPackageManifest>($"{BucketUrl}/{AmongUsVersion}/{ModPackageManifest.ManifestFileName}");
                if (manifest is not null)
                    version = manifest.Version;

            } catch (Exception) { /* ignored */ }
            
            return version;
        }
    }
}