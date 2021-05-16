using System;
using System.IO;
using System.Reactive;
using System.Reactive.Linq;
using System.Windows.Input;
using ClientLauncher.Extensions;
using ClientLauncher.Models;
using ClientLauncher.Services;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace ClientLauncher.ViewModels.LandingPage
{
    public class LandingPageViewModel : ViewModelBase
    {
        public string AmongUsLocation
        {
            get => Context.Configuration.AmongUsLocation;
            set => this.RaiseAndSetIfChanged(ref Context.Configuration.AmongUsLocation, value);
        }
        
        [Reactive]
        public bool IsInstallProgressing { get; set; }
        
        public ICommand ChooseFileCommand { get; }
        public ICommand Autodetect { get; }
        public ICommand InstallGame { get; }
        public Interaction<Unit, string?> ShowDialog { get; }
        public Interaction<string, Unit> WarnDialog { get; }

        public LandingPageViewModel()
        {
            ShowDialog = new Interaction<Unit, string?>();
            WarnDialog = new Interaction<string, Unit>();

            ChooseFileCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                var amongUsLocation = await ShowDialog.Handle(Unit.Default).FirstAsync() ?? AmongUsLocation;

                var gameExists = GameIntegrityService.AmongUsGameExists(new GameInstall
                {
                    Location = amongUsLocation
                });

                if (gameExists)
                    AmongUsLocation = amongUsLocation;
                else
                    await WarnDialog.Handle($"\"{amongUsLocation}\"  is not a valid install location");
            });

            Autodetect = ReactiveCommand.CreateFromTask(async () =>
            {
                AmongUsLocation = await SteamLocatorService.FindAmongUsSteamInstallDirAsync() ?? AmongUsLocation;
            });

            InstallGame = ReactiveCommand.CreateFromTask(async () =>
            {
                var install = new GameInstall
                {
                    Location = AmongUsLocation
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
                        await WarnDialog.Handle($"Couldn't install BepInEx patcher to  \"{install.Location}\"");
                        return;
                    }

                    try
                    {
                        await DownloadService.DownloadPreloaderPatcherAsync(install);
                    }
                    catch (Exception)
                    {
                        // TODO: should be await WarnDialog.Handle()
                        Console.WriteLine($"Couldn't install Polus.gg preloader to  \"{install.Location}\".");
                    }
                    
                    try
                    {
                        foreach (var filePath in GameIntegrityService.FindPolusModFiles(install))
                            File.Delete(filePath);
                        
                        await DownloadService.DownloadPluginsAsync(install);
                    }
                    catch (Exception)
                    {
                        IsInstallProgressing = false;
                        await WarnDialog.Handle($"Couldn't install Polusgg mod to \"{install.Location}\"");
                        return;
                    }

                    await install.LaunchGame();

                }
                catch (Exception e)
                {
                    await WarnDialog.Handle($"Caught exception when launching game: {e.Message}, {e.StackTrace}");
                }
                finally
                {
                    IsInstallProgressing = false;
                }
            });
        }
    }
}