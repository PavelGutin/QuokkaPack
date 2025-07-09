namespace QuokkaPack.Shared.DTOs.Trip
{
    public class TripCreateDto
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Destination { get; set; } = string.Empty;
        public List<int> CategoryIds { get; set; } = [];
    }
}
