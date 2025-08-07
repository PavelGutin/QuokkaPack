namespace QuokkaPack.Shared.DTOs.Trip
{
    public class TripEditDto
    {
        public int Id { get; set; }
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public string Destination { get; set; } = string.Empty;
    }
}
