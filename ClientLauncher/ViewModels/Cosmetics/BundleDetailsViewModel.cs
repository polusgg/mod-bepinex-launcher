using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Media.Imaging;
using ClientLauncher.Models.Cosmetics;
using ClientLauncher.Services;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace ClientLauncher.ViewModels.Cosmetics
{
    public class BundleDetailsViewModel : ViewModelBase
    {
        public string BundleId { get; }
        
        public ObservableCollection<ItemCardViewModel> Items { get; }
        [Reactive] public CosmeticBundle Bundle { get; set; }
        
        [Reactive] public Bitmap KeyArtBitmap { get; set; }
        private string KeyArtCachePath => Path.Combine(Context.CachePath, $"cosmetic_{BundleId}.png");

        public ICommand OnActivated { get; }
        public ICommand OnBuyBundle { get; }
        public ICommand OnCancelBuyBundle { get; }
        
        //TODO: Super sus workaround
        public CosmeticsViewModel ParentViewModel { get; set; }

        public BundleDetailsViewModel(CosmeticsViewModel cosmeticsViewModel, string bundleId)
        {
            ParentViewModel = cosmeticsViewModel;
            BundleId = bundleId;
            Items = new ObservableCollection<ItemCardViewModel>();
            OnActivated = ReactiveCommand.CreateFromTask(OnActivatedAsync);
            OnBuyBundle = ReactiveCommand.CreateFromTask(OnBuyBundleAsync);
            OnCancelBuyBundle = ReactiveCommand.Create(() =>
            {
                ParentViewModel.OnCloseBundleDetails();
            });
        }

        private async Task OnBuyBundleAsync()
        {
            SteamMicroTransactionService.AddMicroTransaction(new PendingMicroTransaction
            {
                PurchaseId = await Context.ApiClient.InitMicroTransaction(BundleId) 
            });
            ParentViewModel.OnCloseBundleDetails();
        }
        
        private async Task OnActivatedAsync()
        {
            var bundle = await Context.ApiClient.GetBundle(BundleId);
            Bundle = bundle;
            await LoadKeyArtAsync();

            foreach (var itemId in bundle.Items)
            {
                try
                {
                    var cosmeticItem = await Context.ApiClient.GetItem(itemId);
                    var itemCardVm = new ItemCardViewModel(cosmeticItem.Name, cosmeticItem.Type, BundleId,
                        cosmeticItem.Id, cosmeticItem.Thumbnail);

                    Items.Add(itemCardVm);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"{e.Message}\n{e.StackTrace}");
                }
            }
        }
        
        private async Task LoadKeyArtAsync()
        {
            try
            {
                if (File.Exists(KeyArtCachePath))
                    KeyArtBitmap = new Bitmap(KeyArtCachePath);

                if (!string.IsNullOrEmpty(Bundle.KeyArtUrl))
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

            await File.WriteAllBytesAsync(KeyArtCachePath, await Context.ApiClient.DownloadImage(Bundle.KeyArtUrl));
        }
    }
}