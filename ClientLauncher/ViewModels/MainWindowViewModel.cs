using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using ClientLauncher.Extensions;
using ClientLauncher.Models;
using ClientLauncher.Services;
using ClientLauncher.Services.GameLocator;
using ClientLauncher.ViewModels.AutoLaunch;
using ClientLauncher.ViewModels.Cosmetics;
using ClientLauncher.ViewModels.LandingPage;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace ClientLauncher.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        [Reactive]
        public ViewModelBase CurrentView { get; set; }
        
        public ICommand OnActivated { get; }
        
        public Interaction<string, Unit> WarnDialog { get; }
        
        public Interaction<bool, Unit> SetInteractivity { get; }


        // TODO: so damn sus
        public static MainWindowViewModel Instance { get; private set; }
        public MainWindowViewModel()
        {
            Instance = this;

            if (Context.IsLaunchedToCosmetics)
                CurrentView = new CosmeticsViewModel();
            else if (Context.Configuration.AutoLaunch)
                CurrentView = new AutoLaunchViewModel();
            else
                CurrentView = new LandingPageViewModel();

            OnActivated = ReactiveCommand.CreateFromTask(OnActivatedAsync);
            
            WarnDialog = new Interaction<string, Unit>();

            SetInteractivity = new Interaction<bool, Unit>();
        }

        public async Task OnActivatedAsync()
        {
            if (CurrentView is not AutoLaunchViewModel)
                return;

            await SetInteractivity.Handle(false);
            
            
            var autodetected = GameAutoDetectService.LocateGameInstallsAsync().Select(x => x.Location).ToArray();

            var vanillaInstall = new GameInstall
            {
                Location = Context.Configuration.VanillaAmongUsLocation
            };
            var moddedInstall = new GameInstall
            {
                Location = Context.ModdedAmongUsLocation
            };
            
            // If not auto launch, any of the new paths are not in old autodetected paths,
            // the modded or vanilla copy doesn't exist, or if the versions are not equal, show the full GUI.
            var showGui = !Context.Configuration.AutoLaunch
                          || !autodetected.All(Context.Configuration.AutodetectedPaths.Contains)
                          || !GameIntegrityService.AmongUsGameExists(vanillaInstall)
                          || !GameIntegrityService.AmongUsGameExists(moddedInstall)
                          || GameVersionService.ParseVersion(vanillaInstall) != GameVersionService.ParseVersion(moddedInstall);

            Context.Configuration.AutodetectedPaths = autodetected;

            if (showGui)
            {
                await ShowLandingPage();
            }
            else
            {
                try
                {
                    await DownloadService.DownloadBepInExAsync(moddedInstall);
                    await DownloadService.DownloadModPackageAsync(moddedInstall);
                    await moddedInstall.LaunchGame();
                    (Application.Current.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.Shutdown();
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Exception while launching game: {e.Message}\n{e.StackTrace}");
                    await WarnDialog.Handle($"Caught exception when launching game.");

                    await ShowLandingPage();
                }
            }
        }

        private async ValueTask ShowLandingPage()
        {
            await SetInteractivity.Handle(true);
            CurrentView = new LandingPageViewModel();
        }

        public void SwitchViewTo(ViewModelBase vm)
        {
            CurrentView = vm;
        }
    }
}