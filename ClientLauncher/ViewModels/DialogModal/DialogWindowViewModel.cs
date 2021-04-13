using ReactiveUI.Fody.Helpers;

namespace ClientLauncher.ViewModels.DialogModal
{
    public class DialogWindowViewModel
    {
        [Reactive] public string DialogString { get; set; } = "";
    }
}