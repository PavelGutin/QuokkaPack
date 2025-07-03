using QuokkaPack.Data.Models;

namespace QuokkaPack.Shared.Models
{
    public class MasterUser
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<UserLogin> Logins { get; set; } = new List<UserLogin>();
        public ICollection<Trip> Trips { get; set; } = new List<Trip>();
    }
}
