using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using ClientLauncher.ViewModels.Cosmetics;
using ReactiveUI;

namespace ClientLauncher.Views.Cosmetics
{
    public class CosmeticsView : ReactiveUserControl<CosmeticsViewModel>
    {
        public CosmeticsView()
        {
            InitializeComponent();

            this.WhenActivated(_ => ViewModel.OnActivated.Execute(null));
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}