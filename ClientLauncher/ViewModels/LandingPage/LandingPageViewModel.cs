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
                AmongUsLocation = GameLocatorService.FindAmongUsSteamInstallDir() ?? AmongUsLocation;
            })

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
                        var currentVerId = GameIntegrityService.BepInExVersion(install);
                        await DownloadService.DownloadBepInEx(install, currentVerId);
                    }
                    catch (Exception)
                    {
                        IsInstallProgressing = false;
                        await WarnDialog.Handle($"Couldn't install BepInEx patcher to  \"{install.Location}\"");
                        return;
                    }

                    try
                    {
                        var preloaderPatcherHash = GameIntegrityService.PreloaderPatcherHash(install);
                        await DownloadService.DownloadPreloaderPatcher(install, preloaderPatcherHash);
                    }
                    catch (Exception)
                    {
                        // TODO: should be await WarnDialog.Handle()
                        Console.WriteLine($"Couldn't install Polus.gg preloader to  \"{install.Location}\".");
                    }
                    
                    try
                    {
                        var downloadable = await DownloadService.GetGamePluginDownloadable(
                            GameIntegrityService.AmongUsVersion(install)
                        );
                    
                        bool hashFound = false;
                        foreach (var filePath in GameIntegrityService.FindPolusModFiles(install))
                        {
                            // If hash has already been found and current file's hash is equal to latest hash,
                            // current file is a duplicate of the already-installed latest plugin version
                            if (FileExtensions.FileEqualsMD5Hash(filePath, downloadable.MD5Hash) || hashFound)
                                File.Delete(filePath);
                            else
                                hashFound = true;
                        }
                        
                        if (!hashFound)
                        {
                            await using var stream = await DownloadService.DownloadPlugin(downloadable);
                            await stream.CopyToAsync(File.OpenWrite(
                                Path.Combine(install.Location, "BepInEx", "plugins", downloadable.DllName)
                            ));
                        }
                    }
                    catch (Exception)
                    {
                        IsInstallProgressing = false;
                        await WarnDialog.Handle($"Couldn't install Polusgg mod to \"{install.Location}\"");
                        return;
                    }

                    await GameLaunchService.LaunchGame(install);

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