using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using Gameloop.Vdf;
using Gameloop.Vdf.Linq;
using Microsoft.Win32;

namespace ClientLauncher.Services
{
    public static class GameLocatorService
    {
        public static string? FindAmongUsSteamInstallDir()
        {
            string steamApps = "";

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                steamApps = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".steam", "steam", "steamapps");
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                steamApps = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Library", "Application Support", "Steam", "steamapps");
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                object steamPath = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Valve\Steam")?.GetValue("SteamPath") ?? "";
                if ((string) steamPath != "")
                    steamApps = Path.Combine((string) steamPath, "steamapps");
                else
                    return null;
            }

            if (steamApps == "" || !Directory.Exists(steamApps))
                return null;

            var libraries = new List<string> { steamApps };

            var vdf = Path.Combine(steamApps, "libraryfolders.vdf");
            if (File.Exists(vdf))
            {
                var libraryFolders = VdfConvert.Deserialize(File.ReadAllText(vdf));

                foreach (var libraryFolder in libraryFolders.Value.Children<VProperty>())
                {
                    if (!int.TryParse(libraryFolder.Key, out _))
                        continue;

                    libraries.Add(Path.Combine(libraryFolder.Value.Value<string>(), "steamapps"));
                }
            }

            foreach (var library in libraries)
            {
                var path = Path.Combine(library, "common", "Among Us");
                if (Directory.Exists(path))
                    return path;
            }
            
            return null;
        }
    }
}