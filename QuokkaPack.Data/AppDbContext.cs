using Microsoft.EntityFrameworkCore;
using QuokkaPack.Data.Models;
using QuokkaPack.Shared.Models;

namespace QuokkaPack.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }
        public DbSet<Trip> Trips => Set<Trip>();
        public DbSet<MasterUser> MasterUsers { get; set; }
        public DbSet<UserLogin> UserLogins { get; set; }
        public DbSet<Item> Items { get; set; }
        public DbSet<Category> Categories { get; set; }
    }
}
