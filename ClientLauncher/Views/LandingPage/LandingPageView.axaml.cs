using System.Reactive;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using Avalonia.VisualTree;
using ClientLauncher.ViewModels.LandingPage;
using ReactiveUI;

namespace ClientLauncher.Views.LandingPage
{
    public class LandingPageView : ReactiveUserControl<LandingPageViewModel>
    {
        public LandingPageView()
        {
            this.WhenActivated(d => d(ViewModel.ShowDialog.RegisterHandler(DoShowDialogAsync)));
            AvaloniaXamlLoader.Load(this);
        }

        private async Task DoShowDialogAsync(InteractionContext<Unit, string?> interactionContext)
        {
            var dialog = new OpenFolderDialog();
            var result = await dialog.ShowAsync((Window) this.GetVisualRoot());
            interactionContext.SetOutput(result);
        }
    }
}