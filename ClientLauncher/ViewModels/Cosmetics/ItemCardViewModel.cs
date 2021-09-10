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
        [Reactive] public Bitmap ThumbnailSecondaryBitmap { get; set; }

        private string ThumbnailCachePath => Path.Combine(Context.CachePath, $"cosmetic_{BundleId}_{ItemId}.png");
        private string ThumbnailSecondaryCachePath => Path.Combine(Context.CachePath, $"cosmetic_{BundleId}_{ItemId}_secondary.png");
        
        // public ICommand OnActivated { get; }
        
        public ItemCardViewModel(string name, string type, string bundleId, string itemId, string thumbnailUrl)
        {
            Name = name;
            Type = type;
            BundleId = bundleId;
            ItemId = itemId;
            ThumbnailUrl = thumbnailUrl;
        }
        
        public async Task LoadKeyArtAsync()
        {
            try
            {
                if (File.Exists(ThumbnailCachePath))
                    ThumbnailBitmap = new Bitmap(ThumbnailCachePath);
                
                if (File.Exists(ThumbnailSecondaryCachePath))
                    ThumbnailSecondaryBitmap = new Bitmap(ThumbnailSecondaryCachePath);

                if (!string.IsNullOrEmpty(ThumbnailUrl))
                {
                    await SaveKeyArtAsync();
                    if (File.Exists(ThumbnailCachePath))
                        ThumbnailBitmap = new Bitmap(ThumbnailCachePath);
                    
                    if (File.Exists(ThumbnailSecondaryCachePath)) 
                        ThumbnailSecondaryBitmap = new Bitmap(ThumbnailSecondaryCachePath);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Exception downloading item thumbnail: {e.Message}\n{e.StackTrace}");
            }
        }

        private async Task SaveKeyArtAsync()
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
                try
                {
                    var image = await Context.ApiClient.DownloadImage($"{ThumbnailUrl}/front.png");
                    await File.WriteAllBytesAsync(ThumbnailCachePath, image);
                    var secondaryImage = await Context.ApiClient.DownloadImage($"{ThumbnailUrl}/back.png"); // TODO: figure out why this fails (item id changes)
                    await File.WriteAllBytesAsync(ThumbnailSecondaryCachePath, secondaryImage);
                }
                catch (Exception) { }

            } else if (Type == "PERK")
            {
                var image = await Context.ApiClient.DownloadImage($"{ThumbnailUrl}");
                await File.WriteAllBytesAsync(ThumbnailCachePath, image);

            }
        }
    }
}