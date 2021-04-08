using System.Reactive;
using System.Reactive.Linq;
using System.Windows.Input;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace ClientLauncher.ViewModels.LandingPage
{
    public class LandingPageViewModel : ViewModelBase
    {
        [Reactive]
        public string AmongUsLocation { get; set; }
        
        public ICommand ChooseFileCommand { get; }
        public Interaction<Unit, string?> ShowDialog { get; }

        public LandingPageViewModel()
        {
            AmongUsLocation = "/home/42069/.steam/root/common/Among Us";
            ShowDialog = new Interaction<Unit, string?>();
            ChooseFileCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                AmongUsLocation = await ShowDialog.Handle(Unit.Default).FirstAsync() ?? AmongUsLocation;
            });
        }
    }
}