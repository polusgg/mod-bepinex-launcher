using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using ClientLauncher.Extensions;
using ClientLauncher.Models;
using ClientLauncher.Services;
using ClientLauncher.Services.GameLocator;
using ClientLauncher.ViewModels.Cosmetics;
using HarmonyLib;
using Newtonsoft.Json;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Steamworks;

namespace ClientLauncher.ViewModels.LandingPage
{
    public class LandingPageViewModel : ViewModelBase
    {
        public string VanillaAmongUsLocation
        {
            get => Context.Configuration.VanillaAmongUsLocation;
            set => this.RaiseAndSetIfChanged(ref Context.Configuration.VanillaAmongUsLocation, value);
        }

        public string[] AutodetectPaths
        {
            get => Context.Configuration.AutodetectedPaths;
            set => this.RaiseAndSetIfChanged(ref Context.Configuration.AutodetectedPaths, value);
        }
        
        public bool AutoLaunch
        {
            get => Context.Configuration.AutoLaunch;
            set => this.RaiseAndSetIfChanged(ref Context.Configuration.AutoLaunch, value);
        }
        
        [Reactive]
        public bool IsInstallProgressing { get; set; }
        
        [Reactive] public string ManifestVersion { get; set; }
        
        public ICommand ChooseFileCommand { get; }
        public ICommand ExecuteAutodetect { get; }
        public ICommand InstallGame { get; }
        
        public ICommand OnClickCosmeticsButton { get; }
        public ICommand OnClickCleanInstall { get; }
        public ICommand OnClickModdedInstallPath { get; }

        public Interaction<Unit, string?> FileChooserDialog { get; }
        public Interaction<string, Unit> WarnDialog { get; }

        public LandingPageViewModel()
        {
            FileChooserDialog = new Interaction<Unit, string?>();
            WarnDialog = new Interaction<string, Unit>();

            ChooseFileCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                var amongUsLocation = await FileChooserDialog.Handle(Unit.Default).FirstAsync();

                if (string.IsNullOrEmpty(amongUsLocation))
                    return;

                var gameExists = GameIntegrityService.AmongUsGameExists(new GameInstall
                {
                    Location = amongUsLocation
                });

                if (gameExists)
                    VanillaAmongUsLocation = amongUsLocation;
                else
                    await WarnDialog.Handle($"\"{amongUsLocation}\"  is not a valid install location");
            });

            ExecuteAutodetect = ReactiveCommand.Create(() =>
            {
                AutodetectPaths = GameAutoDetectService.LocateGameInstallsAsync().Select(x => x.Location).ToArray();
                
                var location = AutodetectPaths.FirstOrDefault(x =>
                    GameIntegrityService.AmongUsGameExists(new GameInstall {Location = x}));
                if (location is not null)
                    VanillaAmongUsLocation = Path.GetFullPath(location);
            }); 

            InstallGame = ReactiveCommand.CreateFromTask(InstallGameAsync);

            OnClickCosmeticsButton = ReactiveCommand.CreateFromTask(async () =>
            {
                try
                {
                    if (File.Exists(Path.Combine(Context.ModdedAmongUsLocation, "api.txt")))
                    {
                        var model = GameVersionService.GetAuthModel();
                        if (model.LoggedInDateTime < DateTime.Now.AddDays(-5))
                        {
                            var response = await Context.ApiClient.CheckToken(model.ClientIdString, model.ClientToken);
                            if (response.Data.ClientToken == model.ClientToken)
                            {
                                await MainWindowViewModel.Instance.SwitchViewTo(new CosmeticsViewModel());
                                return;
                            }
                        }
                        else
                        {
                            await MainWindowViewModel.Instance.SwitchViewTo(new CosmeticsViewModel());
                            return;
                        }
                    }
                    
                    await WarnDialog.Handle("You aren't logged in to Polus.gg in Among Us!");
                }
                catch (Exception e)
                {
                    Console.WriteLine($"{e.Message}\n{e.StackTrace}");
                    await WarnDialog.Handle("You aren't logged in to Polus.gg in Among Us!");
                }
            });

            OnClickCleanInstall = ReactiveCommand.CreateFromTask(async () =>
            {
                if (Directory.Exists(Context.ModdedAmongUsLocation))
                    Directory.Delete(Context.ModdedAmongUsLocation, true);

                await InstallGameAsync();
            });

            OnClickModdedInstallPath = ReactiveCommand.CreateFromTask(async () => Process.Start(new ProcessStartInfo
            {
                UseShellExecute = true,
                Verb = "open",
                FileName = Context.ModdedAmongUsLocation + Path.DirectorySeparatorChar
            }));

            this.WhenAnyValue(x => x.VanillaAmongUsLocation).Subscribe(_ => UpdateManifestVersion());
        }

        private void UpdateManifestVersion()
        {
            var versionString = new List<string>();

            var gameInstall = new GameInstall
            {
                Location = VanillaAmongUsLocation
            };
            if (GameIntegrityService.AmongUsGameExists(gameInstall))
            {
                versionString.Add($"Game Version: {GameVersionService.ParseVersion(gameInstall)}");
            }
            
            var moddedInstall = new GameInstall
            {
                Location = Context.ModdedAmongUsLocation
            };
            if (GameIntegrityService.AmongUsGameExists(moddedInstall) &&
                File.Exists(moddedInstall.ModPackageManifestJson))
            {
                versionString.Add($"Modded Version: {GameVersionService.ParseVersion(moddedInstall)}");
                
                var manifest = JsonConvert.DeserializeObject<ModPackageManifest>(File.ReadAllText(moddedInstall.ModPackageManifestJson));
                if (manifest is not null)
                    versionString.Add($"Package Version: {manifest.Version}");
            }

            ManifestVersion = string.Join(", ", versionString);
        }

        private void AddLauncherInfo() {
            var moddedInstall = new GameInstall
            {
                Location = Context.ModdedAmongUsLocation
            };

            File.WriteAllText(moddedInstall.LauncherClientInfoJson, JsonConvert.SerializeObject(new LauncherClientInfo {
                LauncherVersion = SteamClient.IsValid ? SteamApps.BuildId : -1
            }));
        }

        private async Task InstallGameAsync()
        {
            // Backwards compatibility
            if (SteamClient.IsValid)
            {
                var directoryPath = SteamApps.AppInstallDir(new AppId { Value = 1653240 });
                if (Directory.Exists(directoryPath))
                    Directory.Delete(directoryPath, true);
            }
            
            var vanillaInstall = new GameInstall
            {
                Location = VanillaAmongUsLocation
            };

            var moddedInstall = new GameInstall
            {
                Location = Context.ModdedAmongUsLocation
            };
            
            IsInstallProgressing = true;
            try
            {
#if STEAMRELEASE
                if (Process.GetProcessesByName("Among Us").Length > 0)
                {
                    IsInstallProgressing = false;
                    await WarnDialog.Handle("Among Us is already running!");
                    return;
                }
#endif
                
                
                if (!GameIntegrityService.AmongUsGameExists(vanillaInstall))
                {
                    IsInstallProgressing = false;
                    await WarnDialog.Handle($"\"{vanillaInstall.Location}\" is not a valid install location");
                    return;
                }
                
                if (!GameIntegrityService.AmongUsGameExists(moddedInstall) ||
                    FileExtensions.Sha256Hash(moddedInstall.GameAssemblyDll) != FileExtensions.Sha256Hash(vanillaInstall.GameAssemblyDll))
                {
                    try
                    {
                        await vanillaInstall.CopyTo(moddedInstall);
                    }
                    catch (Exception e)
                    {
                        IsInstallProgressing = false;
                        LoggingService.Log($"Couldn't setup modded install directory: {e.Message}\n{e.StackTrace}");
                        await WarnDialog.Handle("Couldn't setup modded install directory.");
                        return;
                    }
                }
        
                try
                {
                    await DownloadService.DownloadBepInExAsync(moddedInstall);
                }
                catch (Exception e)
                {
                    IsInstallProgressing = false;
                    LoggingService.Log($"Couldn't install BepInEx: {e.Message}\n{e.StackTrace}");
                    await WarnDialog.Handle("Couldn't install BepInEx.");
                    return;
                }
        
                try
                {
                    await DownloadService.DownloadModPackageAsync(moddedInstall);
                }
                catch (Exception e)
                {
                    IsInstallProgressing = false;
                    LoggingService.Log($"Exception installing Polus.gg mod: {e.Message}\n{e.StackTrace}");
                    await WarnDialog.Handle($"Couldn't install Polusgg mod.");
                    return;
                }

                try
                {
                    UpdateManifestVersion();
                    AddLauncherInfo();
                    await moddedInstall.LaunchGame();
                }
                catch (Exception e)
                {
                    LoggingService.Log($"Exception while launching game: {e.Message}\n{e.StackTrace}");
                    await WarnDialog.Handle($"Caught exception when launching game.");
                }
        
            }
            catch (Exception e)
            {
                LoggingService.Log($"Exception while launching game: {e.Message}\n{e.StackTrace}");
                await WarnDialog.Handle($"Unknown error when launching game.");
            }
            finally
            {
                IsInstallProgressing = false;
            }
        }
    }
}