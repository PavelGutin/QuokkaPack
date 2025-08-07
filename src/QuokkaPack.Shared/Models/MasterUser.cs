//using Microsoft.AspNetCore.Identity;

namespace QuokkaPack.Shared.Models
{
    public class MasterUser
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string? IdentityUserId { get; set; }
        //public IdentityUser? IdentityUser { get; set; }
        public ICollection<AppUserLogin> Logins { get; set; } = new List<AppUserLogin>();
        public ICollection<Trip> Trips { get; set; } = new List<Trip>();
        public ICollection<Item> Items { get; set; } = new List<Item>();
        public ICollection<Category> Categories { get; set; } = new List<Category>();
    }
}
