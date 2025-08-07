namespace QuokkaPack.Shared.DTOs.TripItem
{
    public class TripItemCreateDto
    {
        public int ItemId { get; set; } = default!;
        public bool IsPacked { get; set; } = false;
    }
}
