using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using ClientLauncher.Models;

namespace ClientLauncher.Services
{
    public partial class DownloadService
    {
        private static readonly string[] BepinexFiles =
        {
            "winhttp.dll",
            "doorstop_config.ini",
            "mono",
            Path.Combine("BepInEx", "cache"),
            Path.Combine("BepInEx", "core"),
            Path.Combine("BepInEx", "unity-libs"),
            Path.Combine("BepInEx", "unhollowed")
        };

        public static async ValueTask DownloadBepInExAsync(GameInstall install)
        {
            var latest = await Context.GithubClient.Repository.Release
                .GetLatest(Context.BepInExGithubOrg, Context.BepInExGithubRepo);

            if (latest == null)
                throw new InvalidOperationException("Couldn't fetch release for BepInEx");

            if (latest.Id > GameVersionService.BepInExVersion(install))
            {
                var releaseAsset = latest.Assets.First(x => x.Name.Contains("BepInEx"));

                var downloadFilePath = await Context.ApiClient.DownloadFileAsync(releaseAsset.BrowserDownloadUrl);

                CleanInstalledBepInEx(install);
                ZipFile.ExtractToDirectory(downloadFilePath, install.Location);

                await File.WriteAllTextAsync(install.BepInExVersionFile, latest.Id.ToString());
            }
        }

        private static void CleanInstalledBepInEx(GameInstall install)
        {
            foreach (var file in BepinexFiles)
            {
                var fullPath = Path.Combine(install.Location, file);
                if (File.Exists(fullPath))
                    File.Delete(fullPath);

                if (Directory.Exists(fullPath))
                    Directory.Delete(fullPath, true);
            }
        }
    }
}