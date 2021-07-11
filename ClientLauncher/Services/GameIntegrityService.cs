using System.IO;
using System.Linq;
using ClientLauncher.Models;

namespace ClientLauncher.Services
{
    public static class GameIntegrityService
    {
        public static bool AmongUsGameExists(GameInstall install)
        {
            return File.Exists(Path.Combine(install.AmongUsExe)) &&
                   File.Exists(Path.Combine(install.Location, "GameAssembly.dll")) &&
                   File.Exists(Path.Combine(install.Location, "UnityPlayer.dll")) &&
                   Directory.EnumerateFiles(Path.Combine(install.Location, "Among Us_Data")).Any();
        }

        public static readonly string[] BepinexFiles =
        {
            "winhttp.dll",
            "doorstop_config.ini",
            "mono",
            Path.Combine("BepInEx", "core"),
            Path.Combine("BepInEx", "unity-libs"),
        };
        
        public static bool BepInExExists(GameInstall install)
        {
            return BepinexFiles
                .Select(path => Path.Combine(install.Location, path))
                .All(path => File.Exists(path) || Directory.Exists(path));
        }
    }
}