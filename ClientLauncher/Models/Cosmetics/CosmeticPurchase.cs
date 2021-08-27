namespace ClientLauncher.Models.Cosmetics
{
    public class CosmeticPurchase
    {
        public string Id { get; set; }
        public string BundleId { get; set; }
        public uint Cost { get; set; }
        public string Purchaser { get; set; }
        public bool Finalized { get; set; }
    }
}