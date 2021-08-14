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
        public string GameAssemblyDll => Path.Combine(Location, "GameAssembly.dll");
        
        // Unity's globalgamemanagers file containing Among Us version
        public string GlobalGameManagersFile => Path.Combine(Location, "Among Us_Data", "globalgamemanagers");

        // Standard BepInEx paths
        public string BepInExFolder => Path.Combine(Location, "BepInEx");
        public string BepInExMonoFolder => Path.Combine(Location, "mono");

        // Specific paths for downloaded hashes/manifests
        public string BepInExVersionFile => Path.Combine(BepInExFolder, "version.txt");
        public string ModPackageManifestJson => Path.Combine(Location, ModPackageManifest.ManifestFileName);
    }
}