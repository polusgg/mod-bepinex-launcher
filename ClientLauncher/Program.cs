using System;
using System.IO;
using System.Reflection;
using Avalonia;
using Avalonia.ReactiveUI;
using ClientLauncher.Models;
using ClientLauncher.MonkePatches;
using ClientLauncher.Services;
using Newtonsoft.Json;
using Octokit;
using Sentry;
using Sentry.Reflection;
using Steamworks;
using ApiClient = ClientLauncher.Services.Api.ApiClient;

namespace ClientLauncher
{
    class Program
    {
        // Initialization code. Don't use any Avalonia, third-party APIs or any
        // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
        // yet and stuff might break.
        public static void Main(string[] args)
        {
            (Context.MonkePatchLoader = new MonkePatchLoader()).LoadMonkePatches();
            
            Context.GithubClient = new GitHubClient(new ProductHeaderValue("polusgg-client-launcher"));
            Context.ApiClient = new ApiClient();

            using var disposable = SentrySdk.Init(o =>
            {
                o.Dsn = "https://1d7e56371934416e84bd9ddc1596b1ba@o1016669.ingest.sentry.io/5982062";
                o.Release = $"Launcher - v{typeof(Program).Assembly.GetNameAndVersion().Version}";
                // Set traces_sample_rate to 1.0 to capture 100% of transactions for performance monitoring.
                // We recommend adjusting this value in production.
                o.TracesSampleRate = 1.0;
            });
            SentrySdk.StartSession();

            try
            {
                SteamClient.Init(1653240);
                SteamUser.OnMicroTxnAuthorizationResponse += SteamMicroTransactionService.OnMicroTransaction;
            }
            catch (Exception e)
            {
                Console.Write($"Error initializing Steam API: {e.Message}\n{e.StackTrace}");
            }
            

            CreateConfiguration();
            LoggingService.CreateLogFile();
            
            LoggingService.Log(new GameInstall().ModPackageManifestJson);

            BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
            
            
            SaveConfiguration();
            SteamClient.Shutdown();
            Context.ApiClient.Dispose();
        }

        private static void CreateConfiguration()
        {
            try
            {
                Context.Configuration = JsonConvert.DeserializeObject<Configuration>(File.ReadAllText(Context.ConfigPath));

            } catch (Exception) { /* ignored */ }
        }

        private static void SaveConfiguration()
        {
            try
            {
                if (!Directory.Exists(Context.DataPath))
                    Directory.CreateDirectory(Context.DataPath);
                
                if (File.Exists(Context.ConfigPath))
                    File.Delete(Context.ConfigPath);
                
                File.WriteAllText(Context.ConfigPath, JsonConvert.SerializeObject(Context.Configuration, Formatting.Indented));
                
            } catch (Exception) { /* ignored */ }
        }

        // Avalonia configuration, don't remove; also used by visual designer.
        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .LogToTrace()
                .UseReactiveUI();
    }
}