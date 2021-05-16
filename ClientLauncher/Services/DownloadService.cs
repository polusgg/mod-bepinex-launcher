using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using ClientLauncher.Extensions;
using ClientLauncher.Models;

namespace ClientLauncher.Services
{
    public static class DownloadService
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
        
        public static async ValueTask DownloadBepInEx(GameInstall install, uint currentVersionId)
        {
            var latest = await Context.GithubClient?.Repository.Release.GetLatest(Context.BepInExGithubOrg, Context.BepInExGithubRepo);
            
            if (latest == null)
                throw new InvalidOperationException("Couldn't fetch release for BepInEx");

            if (latest.Id > currentVersionId)
            {
                var releaseAsset = latest.Assets.First(x => x.Name.Contains("BepInEx"));
                
                using var httpClient = new HttpClient();
                await using var stream = await httpClient.GetStreamAsync(releaseAsset.BrowserDownloadUrl);
                var downloadPath = await DownloadStreamToTempFile(stream, releaseAsset.Name);

                CleanInstalledBepInEx(install);

                ZipFile.ExtractToDirectory(downloadPath, install.Location);

                var versionTxtPath = Path.Combine(install.Location, "BepInEx", "version.txt");
                await File.WriteAllTextAsync(versionTxtPath, latest.Id.ToString());
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

        public static async ValueTask DownloadPreloaderPatcher(GameInstall install, string preloaderPatcherHash)
        {
            var latest = await Context.GithubClient?.Repository.Release.GetLatest(Context.PreloaderGithubOrg, Context.PreloaderGithubRepo);
            if (latest == null)
                throw new InvalidOperationException("Couldn't fetch release for preloader patcher");
            
            var releaseAsset = latest.Assets.First(x => x.Name.Contains(".dll"));

            using var httpClient = new HttpClient();
            await using var stream = await httpClient.GetStreamAsync(releaseAsset.BrowserDownloadUrl);
            var downloadPath = await DownloadStreamToTempFile(stream, releaseAsset.Name);
            
            await using var downloadedDll = File.OpenRead(downloadPath);
            var newHash = downloadedDll.MD5Hash();
            
            if (preloaderPatcherHash != newHash)
            {
                var preloaderPatchDirectory = Path.Combine(install.Location, "BepInEx", "patchers");
                Directory.Delete(Path.Combine(install.Location, "BepInEx", "patchers"), true);
                Directory.CreateDirectory(preloaderPatchDirectory);

                File.Copy(downloadPath, Path.Combine(preloaderPatchDirectory, releaseAsset.Name));
                await File.WriteAllTextAsync(Path.Combine(preloaderPatchDirectory, "polusgg-preloader.md5hash"), newHash);
            }
        }
        
        public static async Task<GamePluginDownloadable> GetGamePluginDownloadable(string version)
        { 
            var req = WebRequest.CreateHttp($"{Context.BucketUrl}/{version}/manifest.txt");
            var manifest = (await req.GetResponseAsync()).GetResponseStream();

            var array = Encoding.ASCII.GetString(manifest.ReadByteArray()).Split(";");
            return new GamePluginDownloadable
            {
                Version = version,
                DllName = array[0],
                MD5Hash = array[1]
            };
        }

        public static async Task<Stream> DownloadPlugin(GamePluginDownloadable downloadable)
        {
            var req = WebRequest.CreateHttp($"{Context.BucketUrl}/{downloadable.Version}/{downloadable.DllName}");
            return (await req.GetResponseAsync()).GetResponseStream();
        }

        
        private static async Task<string> DownloadStreamToTempFile(Stream inStream, string fileName)
        {
            var tempPathRoot = Path.Combine(Path.GetTempPath(), "polusgg-client-launcher");
            if (File.Exists(tempPathRoot))
                File.Delete(tempPathRoot);

            if (!Directory.Exists(tempPathRoot))
                Directory.CreateDirectory(tempPathRoot);

            var tempPath = Path.Combine(tempPathRoot, fileName);
            if (File.Exists(tempPath))
                File.Delete(tempPath);

            await using var file = File.OpenWrite(tempPath);
            await inStream.CopyToAsync(file);
            
            return tempPath;
        }
    }
}