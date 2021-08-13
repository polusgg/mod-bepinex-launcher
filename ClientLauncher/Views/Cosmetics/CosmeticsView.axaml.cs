using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace ClientLauncher.Views.Cosmetics
{
    public class CosmeticsView : UserControl
    {
        public CosmeticsView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}