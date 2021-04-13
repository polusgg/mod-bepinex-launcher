using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using ClientLauncher.Models;

namespace ClientLauncher.Services
{
    public static class GameLaunchService
    {
        public static async ValueTask LaunchGame(GameInstall install)
        {
            await Task.Yield();
            Process.Start(Path.Combine(install.Location, "Among Us.exe"));
            await Task.Delay(5000);
        }
    }
}