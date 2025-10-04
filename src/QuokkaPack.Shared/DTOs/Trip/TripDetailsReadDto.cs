using QuokkaPack.Shared.DTOs.TripItem;

namespace QuokkaPack.Shared.DTOs.Trip
{
    /// <summary>
    /// Trip details with items that are IN the trip (for viewing/packing)
    /// </summary>
    public class TripDetailsReadDto
    {
        public int Id { get; set; }
        public string Destination { get; set; } = string.Empty;
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public List<TripItemReadDto> Items { get; set; } = new();
    }
}
