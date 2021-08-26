using System.Threading.Tasks;

namespace ClientLauncher.Models.Cosmetics
{
    public class PendingMicroTransaction
    {
        public string PurchaseId { get; set; }
        public TaskCompletionSource<bool> TaskCompletionSource { get; } = new();
    }
}