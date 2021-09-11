using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ClientLauncher.Models;
using Newtonsoft.Json;

namespace ClientLauncher.Services
{
    public partial class DownloadService
    {
        public static async ValueTask DownloadModPackageAsync(GameInstall install)
        {
            var version = GameVersionService.ParseVersion(install);

            var newManifest = await Context.ApiClient.GetModPackageManifestAsync(version);
            
            if (GameVersionService.TryGetModPackageManifest(install, out var manifest))
            {
                if (newManifest.Version > manifest.Version)
                {
                    await FreshInstallAsync(install, manifest, newManifest);
                }
                else
                {
                    if (manifest.Files.Select(x => Path.Combine(install.Location, x.InstallPath)).Any(x => !File.Exists(x) && !Directory.Exists(x)))
                       await FreshInstallAsync(install, manifest, newManifest);
                }
            }
            else
            {
                await InstallModPackageAsync(install, newManifest);
            }
        }

        private static async ValueTask FreshInstallAsync(GameInstall install, ModPackageManifest oldManifest, ModPackageManifest newManifest)
        {
            foreach (var file in oldManifest.Files.Select(x => Path.Combine(install.Location, x.InstallPath)))
            {
                if (File.Exists(file))
                    File.Delete(file);
            }

            await InstallModPackageAsync(install, newManifest);
        }

        private static async ValueTask InstallModPackageAsync(GameInstall install, ModPackageManifest manifest)
        {
            foreach (var file in manifest.Files)
            {
                var downloadedFile = await Context.ApiClient.DownloadFileAsync(file.DownloadUrl);
                var installPath = Path.Combine(install.Location, file.InstallPath);

                var baseDir = Path.GetDirectoryName(installPath);
                if (baseDir is not null)
                    Directory.CreateDirectory(baseDir);

                await File.WriteAllBytesAsync(installPath, downloadedFile.ToArray());
            }
            
            await File.WriteAllTextAsync(install.ModPackageManifestJson, JsonConvert.SerializeObject(manifest));
        }
    }
}