namespace FishBasket
{
    internal class ModConfig
    {
        public string Locale { get; set; } = "auto";
        public bool RequiresBait { get; set; } = false;
        public float BasicJunkMultiplier { get; set; } = 1.0f;
        public float SilverJunkMultiplier { get; set; } = 0.75f;
        public float GoldJunkMultiplier { get; set; } = 0.5f;
        public float IridiumJunkMultiplier { get; set; } = 0.25f;
    }
}
