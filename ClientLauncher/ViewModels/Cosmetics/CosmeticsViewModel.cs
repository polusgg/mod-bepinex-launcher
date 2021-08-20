using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using ClientLauncher.Models.Cosmetics;
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
        
        public ICommand OnActivated { get; }

        public CosmeticsViewModel()
        {
            BundleCards = new ObservableCollection<BundleCardViewModel>();
            OnActivated = ReactiveCommand.CreateFromTask(GetBundleCardsAsync);
            
            this.WhenAnyValue(x => x.CurrentSeletedBundle).Subscribe(x => BundleSelected = x is not null);
        }

        public async Task GetBundleCardsAsync()
        {
            foreach (var bundle in await Context.ApiClient.GetAllBundles())
            {
                BundleCards.Add(new BundleCardViewModel
                {
                    Id = bundle.Id,
                    Name = bundle.Name,
                    Description = bundle.Description,
                    PriceFormatted = $"{bundle.PriceUsd / (double) 100:C}",
                    ForSale = bundle.ForSale,
                    KeyArtUrl = bundle.KeyArtUrl,
                    ParentViewModel = this
                });
            }
        }

        public void OnOpenBundleDetails(string bundleId)
        {
            var bundle = new BundleDetailsViewModel(this, bundleId);
            CurrentSeletedBundle = bundle;
        }
        
        public void OnCloseBundleDetails()
        {
            CurrentSeletedBundle = null;
        }
    }
}