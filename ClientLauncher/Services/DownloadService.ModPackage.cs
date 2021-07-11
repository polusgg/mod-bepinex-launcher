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
                    await FreshInstall(install, newManifest);
                }
                else
                {
                    if (manifest.Files.Select(x => Path.Combine(install.Location, x.InstallPath)).Any(x => !File.Exists(x) && !Directory.Exists(x)))
                       await FreshInstall(install, newManifest);
                }
            }
            else
            {
                await InstallModPackage(install, newManifest);
            }
        }

        private static async ValueTask FreshInstall(GameInstall install, ModPackageManifest manifest)
        {
            foreach (var file in manifest.Files.Select(x => Path.Combine(install.Location, x.InstallPath)))
            {
                if (File.Exists(file))
                    File.Delete(file);
            }

            await InstallModPackage(install, manifest);
        }

        private static async ValueTask InstallModPackage(GameInstall install, ModPackageManifest manifest)
        {
            foreach (var file in manifest.Files)
            {
                var downloadedFile = await Context.ApiClient.DownloadFileAsync(file.DownloadUrl);
                var installPath = Path.Combine(install.Location, file.InstallPath);

                var baseDir = Path.GetDirectoryName(installPath);
                if (baseDir is not null)
                    Directory.CreateDirectory(baseDir);

                downloadedFile.CopyTo(installPath, true);
            }
            
            await File.WriteAllTextAsync(install.ModPackageManifestJson, JsonConvert.SerializeObject(manifest));
        }
    }
}