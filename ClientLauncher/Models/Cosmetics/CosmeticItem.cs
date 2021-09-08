namespace ClientLauncher.Models.Cosmetics
{
    public class CosmeticItem
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Author { get; set; }
        public string AuthorDisplayName { get; set; }
        public uint AmongUsId { get; set; }
        public CosmeticItemResource Resource { get; set; }
        public string Thumbnail { get; set; }
        public string Type { get; set; }
    }
}