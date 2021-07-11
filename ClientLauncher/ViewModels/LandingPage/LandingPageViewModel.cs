using System;
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
        
        [Reactive]
        public bool IsInstallProgressing { get; set; }
        
        public ICommand ChooseFileCommand { get; }
        public ICommand SteamAutodetect { get; }
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

            SteamAutodetect = ReactiveCommand.Create(() =>
            {
                if (SteamLocatorService.TryGetGameLocation(out var gameLocation))
                {
                    if (gameLocation != string.Empty)
                        VanillaAmongUsLocation = gameLocation;
                }
            });

            InstallGame = ReactiveCommand.CreateFromTask(InstallGameAsync);
        }

        private async Task InstallGameAsync()
        {
            var install = new GameInstall
            {
                Location = VanillaAmongUsLocation
            };
        
            IsInstallProgressing = true;
            try
            {
                if (!GameIntegrityService.AmongUsGameExists(install))
                {
                    IsInstallProgressing = false;
                    await WarnDialog.Handle($"\"{install.Location}\"  is not a valid install location");
                    return;
                }
        
                try
                {
                    await DownloadService.DownloadBepInExAsync(install);
                }
                catch (Exception)
                {
                    IsInstallProgressing = false;
                    await WarnDialog.Handle($"Couldn't install BepInEx to ({install.Location})");
                    return;
                }
        
                try
                {
                    await DownloadService.DownloadModPackageAsync(install);
                }
                catch (Exception e)
                {
                    IsInstallProgressing = false;
                    Console.WriteLine($"Execption installing Polus.gg mod: {e.Message}. Stack {e.StackTrace}");
                    await WarnDialog.Handle($"Couldn't install Polusgg mod to \"{install.Location}\"");
                    return;
                }
        
                await install.LaunchGame();
        
            }
            catch (Exception e)
            {
                Console.WriteLine($"Exception while launching game: {e.Message}. Stack {e.StackTrace}");
                await WarnDialog.Handle($"Caught exception when launching game.");
            }
            finally
            {
                IsInstallProgressing = false;
            }
        }
    }
}