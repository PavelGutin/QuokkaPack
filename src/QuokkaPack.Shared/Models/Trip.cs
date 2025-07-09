using QuokkaPack.Shared.Models;

namespace QuokkaPack.Data.Models
{
    public class Trip
    {
        public int Id { get; set; }  
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Destination { get; set; } = string.Empty;
        public Guid MasterUserId { get; set; }
        public MasterUser MasterUser { get; set; } = default!;
        public ICollection<Category> Categories { get; set; } = [];
        public ICollection<TripItem> TripItems { get; set; } = [];
    }
}
