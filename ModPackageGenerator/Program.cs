using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ClientLauncher;
using ClientLauncher.Models;

namespace ModPackageGenerator
{
    static class Program
    {
        static async Task Main(string[] args)
        {
            if (!args.Any())
                Console.WriteLine("No package folder provided!");

            // var bucketUrl = (string) args.GetValue();
            var bucketUrl = Context.BucketUrl;
            var amongUsVersion = (string) args.GetValue(0);
            var packageDirectory = (string) args.GetValue(1);

            if (amongUsVersion is not null && packageDirectory is not null)
            {
                if (Directory.Exists(packageDirectory))
                {
                    var generator = new Generator(packageDirectory, bucketUrl, amongUsVersion);
                    Console.WriteLine($"Generating package from directory ({generator.PackageFolder}) for Among Us version ({generator.AmongUsVersion}) and bucket URL ({generator.BucketUrl})");
                    await generator.GenerateAsync();
                    Console.WriteLine($"Mod package manifest written to ({generator.PackageFolder}/{ModPackageManifest.ManifestFileName})");
                }
                else
                {
                    await Console.Error.WriteLineAsync("Failed, there's no dir!");
                }
            }
            else
            {
                await Console.Error.WriteLineAsync("Failed!");
            }
        }
    }
}