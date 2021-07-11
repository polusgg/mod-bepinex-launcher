using System.Diagnostics;
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
    }
}