namespace QuokkaPack.Shared.Models
{
    public class TripItem
    {
        public int Id { get; set; }
        public int ItemId { get; set; }
        public Item Item { get; set; } = default!;
        public int TripId { get; set; }
        public Trip Trip { get; set; } = default!;
        public bool IsPacked { get; set; } = false;
    }
}