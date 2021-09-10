using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using ClientLauncher.ViewModels.Cosmetics;
using ReactiveUI;

namespace ClientLauncher.Views.Cosmetics
{
    public class ItemCardView : ReactiveUserControl<ItemCardViewModel>
    {
        public ItemCardView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}