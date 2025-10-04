namespace QuokkaPack.Shared.DTOs.Trip
{
    /// <summary>
    /// Lightweight trip summary for list views
    /// </summary>
    public class TripSummaryReadDto
    {
        public int Id { get; set; }
        public string Destination { get; set; } = string.Empty;
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public int TotalItems { get; set; }
        public int PackedItems { get; set; }
    }
}
