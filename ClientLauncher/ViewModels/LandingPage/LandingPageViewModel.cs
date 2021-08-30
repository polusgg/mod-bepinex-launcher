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

            OnClickCosmeticsButton = ReactiveCommand.CreateFromTask(async () => MainWindowViewModel.Instance.SwitchViewTo(new CosmeticsViewModel()));

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

        private async Task InstallGameAsync()
        {
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
                    catch (Exception)
                    {
                        IsInstallProgressing = false;
                        await WarnDialog.Handle($"Couldn't setup modded install directory.");
                        return;
                    }
                }
        
                try
                {
                    await DownloadService.DownloadBepInExAsync(moddedInstall);
                }
                catch (Exception)
                {
                    IsInstallProgressing = false;
                    await WarnDialog.Handle($"Couldn't install BepInEx.");
                    return;
                }
        
                try
                {
                    await DownloadService.DownloadModPackageAsync(moddedInstall);
                }
                catch (Exception e)
                {
                    IsInstallProgressing = false;
                    Console.WriteLine($"Execption installing Polus.gg mod: {e.Message}\n{e.StackTrace}");
                    await WarnDialog.Handle($"Couldn't install Polusgg mod.");
                    return;
                }

                UpdateManifestVersion();
                await moddedInstall.LaunchGame();
        
            }
            catch (Exception e)
            {
                Console.WriteLine($"Exception while launching game: {e.Message}\n{e.StackTrace}");
                await WarnDialog.Handle($"Caught exception when launching game.");
            }
            finally
            {
                IsInstallProgressing = false;
            }
        }
    }
}