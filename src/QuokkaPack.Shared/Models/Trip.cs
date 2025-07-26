using QuokkaPack.Shared.Models;

namespace QuokkaPack.Data.Models
{
    public class Trip
    {
        public int Id { get; set; }  
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public string Destination { get; set; } = string.Empty;
        public Guid MasterUserId { get; set; }
        public MasterUser MasterUser { get; set; } = default!;
        public ICollection<TripItem> TripItems { get; set; } = new List<TripItem>();
    }
}
