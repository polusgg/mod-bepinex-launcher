using System;
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
        
        public ICommand ChooseFileCommand { get; }
        public ICommand ExecuteAutodetect { get; }
        public ICommand InstallGame { get; }
        
        public Interaction<Unit, string?> FileChooserDialog { get; }
        public Interaction<string, Unit> WarnDialog { get; }

        public LandingPageViewModel()
        {
            FileChooserDialog = new Interaction<Unit, string?>();
            WarnDialog = new Interaction<string, Unit>();

            ChooseFileCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                var amongUsLocation = await FileChooserDialog.Handle(Unit.Default).FirstAsync() ?? VanillaAmongUsLocation;

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
                    VanillaAmongUsLocation = location;
            }); 

            InstallGame = ReactiveCommand.CreateFromTask(InstallGameAsync);
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
                if (!GameIntegrityService.AmongUsGameExists(vanillaInstall))
                {
                    IsInstallProgressing = false;
                    await WarnDialog.Handle($"\"{vanillaInstall.Location}\" is not a valid install location");
                    return;
                }
                
                if (!GameIntegrityService.AmongUsGameExists(moddedInstall) || GameVersionService.ParseVersion(vanillaInstall) != GameVersionService.ParseVersion(moddedInstall))
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