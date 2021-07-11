using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ClientLauncher.Models;

namespace ModPackageGenerator
{
    static class Program
    {
        static async Task Main(string[] args)
        {
            if (!args.Any())
                Console.WriteLine("No package folder provided!");

            var bucketUrl = (string) args.GetValue(0);
            var amongUsVersion = (string) args.GetValue(1);
            var packageDirectory = (string) args.GetValue(2);

            if (bucketUrl is not null && amongUsVersion is not null && packageDirectory is not null)
            {
                if (Directory.Exists(packageDirectory))
                {
                    var generator = new Generator(packageDirectory, bucketUrl, amongUsVersion);
                    Console.WriteLine($"Generating package from directory ({generator.PackageFolder}) for Among Us version ({generator.AmongUsVersion}) and bucket URL ({generator.BucketUrl})");
                    await generator.GenerateAsync();
                    Console.WriteLine($"Mod package manifest written to ({generator.PackageFolder}/{ModPackageManifest.ManifestFileName})");
                }
            }
        }
    }
}