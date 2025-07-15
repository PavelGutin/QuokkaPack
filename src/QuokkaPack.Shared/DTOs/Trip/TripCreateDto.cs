namespace QuokkaPack.Shared.DTOs.Trip
{
    public class TripCreateDto
    {
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public string Destination { get; set; } = string.Empty;
        public List<int> CategoryIds { get; set; } = [];
    }
}
