using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Media.Imaging;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace ClientLauncher.ViewModels.Cosmetics
{
    public class ItemCardViewModel : ViewModelBase
    {
        [Reactive] public string Name { get; set; }
        [Reactive] public string Type { get; set; }

        public string BundleId { get; set; }
        public string ItemId { get; set; }
        public string ThumbnailUrl { get; set; }
        
        [Reactive] public Bitmap ThumbnailBitmap { get; set; }
        private string ThumbnailCachePath => Path.Combine(Context.CachePath, $"cosmetic_{BundleId}_{ItemId}.png");

        public ICommand OnActivated { get; }
        
        public ItemCardViewModel(string name, string type, string bundleId, string itemId, string thumbnailUrl)
        {
            Name = name;
            Type = type;
            BundleId = bundleId;
            ItemId = itemId;
            ThumbnailUrl = thumbnailUrl;

            OnActivated = ReactiveCommand.CreateFromTask(LoadKeyArtAsync);
        }
        
        public async Task LoadKeyArtAsync()
        {
            try
            {
                if (File.Exists(ThumbnailCachePath))
                    ThumbnailBitmap = new Bitmap(ThumbnailCachePath);

                if (!string.IsNullOrEmpty(ThumbnailUrl))
                {
                    await SaveKeyArtAsync();
                    ThumbnailBitmap = new Bitmap(ThumbnailCachePath);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Exception downloading item thumbnail: {e.Message}\n{e.StackTrace}");
            }
        }

        private async ValueTask SaveKeyArtAsync()
        {
            if (!Directory.Exists(Context.CachePath))
                Directory.CreateDirectory(Context.CachePath);

            //TODO: thumbnail generation
            if (Type == "PET")
            {
                await File.WriteAllBytesAsync(ThumbnailCachePath, await Context.ApiClient.DownloadImage($"{ThumbnailUrl}/{Name}/pet.png"));
            }
            else if (Type == "HAT")
            {
                await File.WriteAllBytesAsync(ThumbnailCachePath, await Context.ApiClient.DownloadImage($"{ThumbnailUrl}/{Name}/front.png"));
            }
        }
    }
}