using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.X11;
using ClientLauncher.Models.Cosmetics;
using ClientLauncher.Services;
using ClientLauncher.ViewModels.LandingPage;
using DynamicData;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace ClientLauncher.ViewModels.Cosmetics
{
    public class CosmeticsViewModel : ViewModelBase
    {
        public ObservableCollection<BundleCardViewModel> BundleCards { get; }
        
        //TODO: Super sus
        [Reactive] public BundleDetailsViewModel CurrentSeletedBundle { get; set; }
        [Reactive] public bool BundleSelected { get; set; }
        [Reactive] public bool AttemptedLoad { get; set; }
        
        public ICommand OnClickLauncherButton { get; }
        public ICommand OnClickRefundButton { get; }
        
        public ICommand OnActivated { get; }

        public CosmeticsViewModel()
        {
            BundleCards = new ObservableCollection<BundleCardViewModel>();
            OnActivated = ReactiveCommand.CreateFromTask(GetBundleCardsAsync);

            AttemptedLoad = false;

            OnClickLauncherButton =
                ReactiveCommand.CreateFromTask(async () => MainWindowViewModel.Instance.SwitchViewTo(new LandingPageViewModel()));
            OnClickRefundButton = ReactiveCommand.Create(() =>
                PlatformUtils.OpenUrl("https://store.steampowered.com/account/subscriptions/"));
            
            this.WhenAnyValue(x => x.CurrentSeletedBundle).Subscribe(x => BundleSelected = x is not null);
        }

        public async Task GetBundleCardsAsync()
        {
            try
            {
                var purchased = await Context.ApiClient.GetPurchases();
                foreach (var bundle in await Context.ApiClient.GetAllBundles())
                {
                    if (purchased.All(x => !x.Finalized || x.BundleId != bundle.Id))
                    {
                        var priceDecimal = Math.Round(bundle.PriceUsd / (double)100, 2);

                        String formattedText;
                        if (bundle.PriceUsd <= 0)
                        {
                            formattedText = "Claim";
                        }
                        else if (bundle.Recurring)
                        {
                            formattedText = $"${priceDecimal}/mo";
                        }
                        else
                        {
                            formattedText = $"Buy - ${priceDecimal}";
                        }

                        BundleCards.Add(new BundleCardViewModel
                        {
                            Id = bundle.Id,
                            Name = bundle.Name,
                            Description = bundle.Description,
                            PriceFormatted = formattedText,
                            ForSale = bundle.ForSale,
                            KeyArtUrl = bundle.KeyArtUrl,
                            ParentViewModel = this
                        });
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error fetching bundles from cosmetics server..");
            }
            finally
            {
                AttemptedLoad = true;
            }
        }

        public void OnOpenBundleDetails(string bundleId)
        {
            var bundle = new BundleDetailsViewModel(this, bundleId);
            CurrentSeletedBundle = bundle;
        }
        
        public void OnCloseBundleDetails(bool success)
        {
            if (success)
                BundleCards.Remove(BundleCards.First(x => x.Id == CurrentSeletedBundle.BundleId));

            CurrentSeletedBundle = null;
        }
    }
}