namespace QuokkaPack.Shared.DTOs.Trip
{
    public class TripSummaryReadDto
    {
        public int Id { get; set; }
        public string Destination { get; set; } = string.Empty;
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        //public List<string> Categories { get; set; } = [];
    }
}
