using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using ClientLauncher.Models;
using Sentry;

namespace ClientLauncher.Services
{
    public partial class DownloadService
    {
        public static async ValueTask DownloadBepInExAsync(GameInstall install)
        {
            var latest = await Context.GithubClient.Repository.Release
                .GetLatest(Context.BepInExGithubOrg, Context.BepInExGithubRepo);

            if (latest is null)
                throw new InvalidOperationException("Couldn't fetch release for BepInEx");

            // TODO: does not check the case where currently installed version does not have all files
            Console.WriteLine(latest.Id);
            Console.WriteLine(GameVersionService.BepInExVersion(install));
            if (latest.Id > GameVersionService.BepInExVersion(install))
            {
                var releaseAsset = latest.Assets.First(x => x.Name.Contains("BepInEx"));

                await using var zipStream = await Context.ApiClient.DownloadFileAsync(releaseAsset.BrowserDownloadUrl);

                if (Directory.Exists(install.BepInExFolder))
                    Directory.Delete(install.BepInExFolder, true);

                Directory.CreateDirectory(install.BepInExFolder);

                using var zip = new ZipArchive(zipStream);
                foreach (ZipArchiveEntry entry in zip.Entries)
                {
                    var zipEntryPath = Path.GetFullPath(Path.Combine(install.Location, entry.FullName));
                    try
                    {

                        var directoryName = Path.GetDirectoryName(zipEntryPath);
                        if (directoryName is not null)
                            Directory.CreateDirectory(directoryName);
                        
                        if (entry.FullName == "winhttp.dll")
                            await File.WriteAllTextAsync(zipEntryPath, "placeholder");
                            
                        entry.ExtractToFile(zipEntryPath, true);
                    }
                    catch (Exception e)
                    {
                        LoggingService.Log($"Couldn't extract file {entry.FullName} to path {zipEntryPath}: {e.Message}\n{e.StackTrace}");   
                        SentrySdk.CaptureException(e);
                    }
                }


                await File.WriteAllTextAsync(install.BepInExVersionFile, latest.Id.ToString());
            }
        }
    }
}