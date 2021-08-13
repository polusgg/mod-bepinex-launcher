using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace ClientLauncher.Views.AutoLaunch
{
    public class AutoLaunchView : UserControl
    {
        public AutoLaunchView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}