using System;
using System.IO;

namespace ClientLauncher.Models
{
    public class GameInstall
    {
        public string Location { get; set; } =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), ".polusgg");
        
        
        /*
         *  Path Properties 
         */
        
        public string AmongUsExe => Path.Combine(Location, "Among Us.exe");
        
        // Unity's globalgamemanagers file containing Among Us version
        public string GlobalGameManagersFile => Path.Combine(Location, "Among Us_Data", "globalgamemanagers");

        // Standard BepInEx paths
        public string BepInExFolder => Path.Combine(Location, "BepInEx");
        public string PreloaderFolder => Path.Combine(BepInExFolder, "patchers");
        public string PluginFolder => Path.Combine(BepInExFolder, "plugins");

        // Specific paths for downloaded asset hashes
        public string BepInExVersionFile => Path.Combine(BepInExFolder, "version.txt");
        public string PreloaderHashFile => Path.Combine(PreloaderFolder, "polusgg-preloader.sha256hash");
    }
}