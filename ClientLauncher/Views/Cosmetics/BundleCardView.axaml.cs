using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using ClientLauncher.ViewModels.Cosmetics;
using ReactiveUI;

namespace ClientLauncher.Views.Cosmetics
{
    public class BundleCardView : ReactiveUserControl<BundleCardViewModel>
    {
        public BundleCardView()
        {
            InitializeComponent();
            this.WhenActivated(_ => ViewModel.OnActivated.Execute(null));
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void Button_OnClick(object? sender, RoutedEventArgs e)
        {
            ViewModel.OnOpenBundleDetails.Execute(null);
            RaiseEvent(new RoutedEventArgs
            {
                RoutedEvent = OpenBundleDetailsEvent,
                Route = RoutingStrategies.Bubble
            });
        }
        
        
        public static readonly RoutedEvent<RoutedEventArgs> OpenBundleDetailsEvent =
            RoutedEvent.Register<BundleCardView, RoutedEventArgs>(nameof(OpenBundleDetails), RoutingStrategies.Bubble);

        public event EventHandler<RoutedEventArgs> OpenBundleDetails
        {
            add => AddHandler(OpenBundleDetailsEvent, value);
            remove => RemoveHandler(OpenBundleDetailsEvent, value);
        }
    }
}