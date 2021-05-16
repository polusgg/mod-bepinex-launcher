using System.IO;
using ClientLauncher.Extensions;
using ClientLauncher.Models;

namespace ClientLauncher.Services
{
    public class GameVersionService
    {
        public static uint BepInExVersion(GameInstall install)
        {
            if (GameIntegrityService.BepInExExists(install))
            {
                if (File.Exists(install.BepInExVersionFile) &&
                    uint.TryParse(File.ReadAllText(install.BepInExVersionFile), out uint versionId))
                    return versionId;
            }

            return 0;
        }

        public static string PreloaderPatcherHash(GameInstall install)
        {
            return GameIntegrityService.PreloaderPluginExists(install) ? File.ReadAllText(install.PreloaderHashFile) : "";
        }
        
        public static string AmongUsVersion(GameInstall install)
        {
            return GameIntegrityService.AmongUsGameExists(install) ? install.ParseVersion() : "";
        }
    }
}