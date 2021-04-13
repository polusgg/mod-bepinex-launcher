using System.Collections.Generic;
using System.IO;
using System.Linq;
using ClientLauncher.Models;
using ModClientPreloader.Services;

namespace ClientLauncher.Services
{
    public static class GameIntegrityService
    {
        public static bool AmongUsGameExists(GameInstall install)
        {
            return File.Exists(Path.Combine(install.Location, "Among Us.exe")) &&
                   File.Exists(Path.Combine(install.Location, "GameAssembly.dll")) &&
                   Directory.EnumerateFiles(Path.Combine(install.Location, "Among Us_Data")).Any();
        }

        public static bool BepInExExists(GameInstall install)
        {
            return File.Exists(Path.Combine(install.Location, "winhttp.dll")) &&
                Directory.EnumerateFileSystemEntries(Path.Combine(install.Location, "mono")).Any() &&
                Directory.EnumerateFileSystemEntries(Path.Combine(install.Location, "BepInEx")).Any();
        }
        
        public static bool PreloaderPluginExists(GameInstall install)
        {
            return File.Exists(Path.Combine(install.Location, "BepInEx", "patchers", "polusgg-preloader.md5hash"));
        }

        public static uint BepInExVersion(GameInstall install)
        {
            if (BepInExExists(install))
            {
                var versionFile = Path.Combine(install.Location, "BepInEx", "version.txt");
                if (File.Exists(versionFile) &&
                    uint.TryParse(File.ReadAllText(versionFile), out uint versionId))
                    return versionId;
            }

            return 0;
        }

        public static string PreloaderPatcherHash(GameInstall install)
        {
            if (PreloaderPluginExists(install))
                return File.ReadAllText(
                    Path.Combine(install.Location, "BepInEx", "patchers", "polusgg-preloader.md5hash"));
            return "";
        }
        
        public static string AmongUsVersion(GameInstall install)
        {
            if (AmongUsGameExists(install))
                return GameVersionParser.Parse(Path.Combine(install.Location, "Among Us_Data", "globalgamemanagers"));
            return "";
        }
        
        public static IEnumerable<string> FindPolusModFiles(GameInstall install)
        {
            var pluginPath = Path.Combine(install.Location, "BepInEx", "plugins");
            if (!Directory.Exists(pluginPath))
                Directory.CreateDirectory(pluginPath);

            return Directory.EnumerateFiles(pluginPath)
                .Where(fileName => fileName.ToLower().Contains(Context.PolusModPrefix))
                .Select(file => Path.Combine(pluginPath, file));
        }
    }
}