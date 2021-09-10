using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ClientLauncher.Models;

namespace ClientLauncher.Extensions
{
    public static class GameInstallExtensions
    {
        public static async ValueTask LaunchGame(this GameInstall install)
        {
            await Task.Yield();
            Process.Start(install.AmongUsExe);
            await Task.Delay(5000);
        }
        
        public static async ValueTask CopyTo(this GameInstall install, string newInstallPath)
        {
            if (File.Exists(newInstallPath))
                File.Delete(newInstallPath);
            
            if (Directory.Exists(newInstallPath))
                Directory.Delete(newInstallPath, true);

            var basePath = Path.GetFullPath(install.Location);
            foreach (var filePath in Directory.EnumerateFiles(install.Location, "*", SearchOption.AllDirectories).Select(Path.GetFullPath))
            {
                if (Directory.Exists(filePath) && Path.GetFileName(filePath) != "Among Us_Data")
                    continue;

                var rebasedPath = filePath.Replace(basePath, "").TrimStart('/');
                rebasedPath = Path.Join(newInstallPath, rebasedPath);

                var baseDir = Path.GetDirectoryName(rebasedPath);
                if (baseDir is not null)
                    Directory.CreateDirectory(baseDir);
                
                File.Copy(filePath, rebasedPath, true);
            }
        }
        
        public static async ValueTask CopyTo(this GameInstall install, GameInstall otherInstall)
        {
            await install.CopyTo(otherInstall.Location);
        }
    }
}