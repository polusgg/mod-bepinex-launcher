using System;
using System.Reactive;
using System.Reactive.Disposables;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using Avalonia.VisualTree;
using ClientLauncher.ViewModels;
using ClientLauncher.ViewModels.DialogModal;
using ClientLauncher.Views.DialogModal;
using ReactiveUI;
using Steamworks;

namespace ClientLauncher.Views
{
    public class MainWindow : ReactiveWindow<MainWindowViewModel>
    {
        public MainWindow()
        {
            this.WhenActivated(block =>
            {
                ViewModel.WarnDialog.RegisterHandler(DoWarnDialogAsync).DisposeWith(block);
                ViewModel.SetInteractivity.RegisterHandler(DoInteractivityToggleAsync).DisposeWith(block);
                
                ViewModel.OnActivated.Execute(null);
            });
            AvaloniaXamlLoader.Load(this);
#if DEBUG
            this.AttachDevTools();
#endif
        }

        private async Task DoWarnDialogAsync(InteractionContext<string, Unit> context)
        {
            var dialog = new DialogWindow
            {
                DataContext = new DialogWindowViewModel
                {
                    DialogString = context.Input
                },
            };
            await dialog.ShowDialog<Unit>((Window) this.GetVisualRoot());
            context.SetOutput(Unit.Default);
        }
        
        private async Task DoInteractivityToggleAsync(InteractionContext<bool, Unit> context)
        {
            SystemDecorations = context.Input ? SystemDecorations.Full : SystemDecorations.None;
            context.SetOutput(Unit.Default);
        }
    }
}