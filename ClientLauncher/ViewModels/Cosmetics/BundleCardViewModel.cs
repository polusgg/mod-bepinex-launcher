using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Media.Imaging;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace ClientLauncher.ViewModels.Cosmetics
{
    public class BundleCardViewModel : ViewModelBase
    {
        public string Id { get; set; } = string.Empty;
        [Reactive] public string Name { get; set; } = string.Empty;
        [Reactive] public string Description { get; set; } = string.Empty;
        [Reactive] public bool ForSale { get; set; }
        [Reactive] public string PriceFormatted { get; set; } = string.Empty;
        public string KeyArtUrl { get; set; } = string.Empty;
        

        [Reactive] public Bitmap KeyArtBitmap { get; set; }
        private string KeyArtCachePath => Path.Combine(Context.CachePath, $"cosmetic_{Id}.png");
        
        public ICommand OnActivated { get; }
        
        //TODO: Super sus workaround
        public CosmeticsViewModel ParentViewModel { get; set; }
        public ICommand OnOpenBundleDetails { get; set; }
        
        public BundleCardViewModel()
        {
            OnActivated = ReactiveCommand.CreateFromTask(LoadKeyArtAsync);
            
            
            OnOpenBundleDetails = ReactiveCommand.CreateFromTask(async () =>
            {
                ParentViewModel?.OnOpenBundleDetails(Id);
            });
        }

        private async Task LoadKeyArtAsync()
        {
            try
            {
                if (File.Exists(KeyArtCachePath))
                    KeyArtBitmap = new Bitmap(KeyArtCachePath);

                if (!string.IsNullOrEmpty(KeyArtUrl))
                {
                    await SaveKeyArtAsync();
                    KeyArtBitmap = new Bitmap(KeyArtCachePath);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Exception downloading bundle key art: {e.Message}\n{e.StackTrace}");
            }
        }

        private async ValueTask SaveKeyArtAsync()
        {
            if (!Directory.Exists(Context.CachePath))
                Directory.CreateDirectory(Context.CachePath);

            await File.WriteAllBytesAsync(KeyArtCachePath, await Context.ApiClient.DownloadImage(KeyArtUrl));
        }
    }
}