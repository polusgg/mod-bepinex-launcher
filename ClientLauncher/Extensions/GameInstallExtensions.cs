using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClientLauncher.Models;

namespace ClientLauncher.Extensions
{
    public static class GameInstallExtensions
    {
        public static string ParseVersion(this GameInstall install)
        {
            var bytes = File.ReadAllBytes(install.GlobalGameManagersFile);

            var pattern = Encoding.UTF8.GetBytes("public.app-category.games");
            var index = bytes.IndexOfPattern(pattern) + pattern.Length + 127;

            return Encoding.UTF8.GetString(bytes.Skip(index).TakeWhile(x => x != 0).ToArray());
        }

        public static async ValueTask LaunchGame(this GameInstall install)
        {
            await Task.Yield();
            Process.Start(install.AmongUsExe);
            await Task.Delay(5000);
        }
    }
}