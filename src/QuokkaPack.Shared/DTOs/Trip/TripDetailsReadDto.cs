using QuokkaPack.Shared.DTOs.TripCatalogItem;

namespace QuokkaPack.Shared.DTOs.Trip
{
    public class TripDetailsReadDto
    {
        public int Id { get; set; }
        public string Destination { get; set; } = string.Empty;
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public List<TripCatalogItemReadDto> Items { get; set; }
    }
}
