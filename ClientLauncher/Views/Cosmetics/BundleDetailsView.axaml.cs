using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using ClientLauncher.ViewModels.Cosmetics;
using ReactiveUI;

namespace ClientLauncher.Views.Cosmetics
{
    public class BundleDetailsView : ReactiveUserControl<BundleDetailsViewModel>
    {
        public BundleDetailsView()
        {
            InitializeComponent();
            this.WhenActivated(_ => ViewModel.OnActivated.Execute(null));
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void BuyButton_OnClick(object? sender, RoutedEventArgs e)
        {
            ViewModel.OnBuyBundle.Execute(null);
        }
        
        private void CancelButton_OnClick(object? sender, RoutedEventArgs e)
        {
            ViewModel.OnCancelBuyBundle.Execute(null);
        }
    }
}