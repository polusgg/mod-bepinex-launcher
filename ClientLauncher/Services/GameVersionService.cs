using System;
using System.IO;
using System.Linq;
using System.Text;
using ClientLauncher.Extensions;
using ClientLauncher.Models;
using Newtonsoft.Json;

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

        public static bool TryGetModPackageManifest(GameInstall install, out ModPackageManifest modPackageManifest)
        {
            modPackageManifest = new ModPackageManifest();
            if (File.Exists(install.ModPackageManifestJson))
            {
                var deserialized = JsonConvert.DeserializeObject<ModPackageManifest>(File.ReadAllText(install.ModPackageManifestJson));
                if (deserialized != null)
                {
                    modPackageManifest = deserialized;
                    return true;
                }
            }
            return false;
        }
        
        public static string ParseVersion(GameInstall install)
        {
            var bytes = File.ReadAllBytes(install.GlobalGameManagersFile);

            var pattern = Encoding.UTF8.GetBytes("public.app-category.games");
            var index = bytes.IndexOfPattern(pattern) + pattern.Length + 127;

            return Encoding.UTF8.GetString(bytes.Skip(index).TakeWhile(x => x != 0).ToArray());
        }
        
        public static SavedAuthModel GetAuthModel()
        {
            var model = JsonConvert.DeserializeObject<SavedAuthModel>(
                Encoding.UTF8.GetString(Convert.FromBase64String(File.ReadAllText(Path.Combine(Context.ModdedAmongUsLocation, "api.txt"))))
            );

            if (model is null)
                throw new InvalidOperationException(
                    "Could not find api.txt from Among Us, are you logged in on the client?");
            return model;
        }
    }
}