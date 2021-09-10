using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
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
        
        [Reactive] public string BuyButtonText { get; set; }
        
        [Reactive] public string Authors { get; set; }
        
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
                ParentViewModel.OnCloseBundleDetails(false);
            });
        }

        private async Task OnBuyBundleAsync()
        {
            var response = await Context.ApiClient.InitMicroTransaction(BundleId);
            if (!response.Ok)
            { 
                Console.WriteLine("Error from SteamAPI initializing the microtransaction.");
                return;
            }

            var success = await SteamMicroTransactionService
                .ProcessMicroTransaction(new PendingMicroTransaction 
                { 
                    PurchaseId = response.PurchaseId 
                });

            ParentViewModel.OnCloseBundleDetails(success);
        }
        
        private async Task OnActivatedAsync()
        {
            var bundle = await Context.ApiClient.GetBundle(BundleId);
            Bundle = bundle;

            var priceDecimal = Math.Round(bundle.PriceUsd / (double)100, 2);

            String formattedText;
            if (bundle.PriceUsd <= 0)
            {
                formattedText = "Claim";
            } else if (bundle.Recurring)
            {
                formattedText = $"Subscribe - ${priceDecimal}/mo";
            }
            else
            {
                formattedText = $"Buy - ${priceDecimal}";
            }
            
            BuyButtonText = formattedText;
            
            await LoadKeyArtAsync();

            HashSet<string> authors = new();
            foreach (var itemId in bundle.Items)
            {
                try
                {
                    var cosmeticItem = await Context.ApiClient.GetItem(itemId);
                    authors.Add(cosmeticItem.AuthorDisplayName);
                    var itemCardVm = new ItemCardViewModel(cosmeticItem.Name, cosmeticItem.Type, BundleId,
                        cosmeticItem.Id, cosmeticItem.Thumbnail);

                    Items.Add(itemCardVm);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"{e.Message}\n{e.StackTrace}");
                }
            }

            Authors = string.Join(", ", authors);
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