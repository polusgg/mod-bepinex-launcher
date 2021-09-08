namespace ClientLauncher.Models.Cosmetics
{
    public class CosmeticBundle
    {
        public string Id { get; set; }
        public string KeyArtUrl { get; set; }
        public string Color { get; set; }
        public string Name { get; set; }
        public string[] Items { get; set; }
        public uint PriceUsd { get; set; }
        public string Description { get; set; }
        public bool ForSale { get; set; }
        
        public bool Recurring { get; set; }
    }
}