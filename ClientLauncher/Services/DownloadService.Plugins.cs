using System;
using System.IO;
using System.Threading.Tasks;
using ClientLauncher.Extensions;
using ClientLauncher.Models;

namespace ClientLauncher.Services
{
    public partial class DownloadService
    {
        public static async ValueTask DownloadPluginsAsync(GameInstall install)
        {
            var version = install.ParseVersion();

            var manifest = await Context.ApiClient.GetPluginDownloadManifestAsync(version);
            if (manifest == null)
                throw new InvalidOperationException($"Download manifest.json was not found for version {version}");

            foreach (var plugin in manifest.Plugins)
                await DownloadPlugin(install.PluginFolder, plugin);
        }

        private static async ValueTask DownloadPlugin(string pluginFolder, DownloadablePlugin downloadablePlugin)
        {
            // TODO: Refactor download and copy to single stream operation
            var downloadFilePath = await Context.ApiClient.DownloadFileAsync(downloadablePlugin.DownloadUrl);
            File.Copy(downloadFilePath, Path.Combine(pluginFolder, downloadablePlugin.DllName));
        }
    }
}