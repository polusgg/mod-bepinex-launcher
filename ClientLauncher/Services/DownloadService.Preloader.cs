using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using ClientLauncher.Extensions;
using ClientLauncher.Models;

namespace ClientLauncher.Services
{
    public partial class DownloadService
    {
        public static async ValueTask DownloadPreloaderPatcherAsync(GameInstall install)
        {
            var latest = await Context.GithubClient.Repository.Release
                .GetLatest(Context.PreloaderGithubOrg, Context.PreloaderGithubRepo);
            if (latest == null)
                throw new InvalidOperationException("Couldn't fetch release for preloader patcher");

            var releaseAsset = latest.Assets.First(x => x.Name.Contains(".dll"));

            var downloadFilePath = await Context.ApiClient.DownloadFileAsync(releaseAsset.BrowserDownloadUrl);
            var downloadFileHash = FileExtensions.SHA256Hash(downloadFilePath);
            
            if (downloadFileHash != GameVersionService.PreloaderPatcherHash(install))
            {
                Directory.Delete(install.PreloaderFolder, true);
                Directory.CreateDirectory(install.PreloaderFolder);

                File.Copy(downloadFilePath, Path.Combine(install.PreloaderFolder, releaseAsset.Name));
                await File.WriteAllTextAsync(install.PreloaderHashFile, downloadFileHash);
            }
        }
    }
}