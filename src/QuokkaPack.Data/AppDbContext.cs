using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

using QuokkaPack.Shared.Models;

namespace QuokkaPack.Data
{
    public class AppDbContext : IdentityDbContext<IdentityUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }
        public DbSet<AppUserLogin> AppUserLogins { get; set; } = default!;
        public DbSet<Trip> Trips => Set<Trip>();
        public DbSet<MasterUser> MasterUsers { get; set; }
        public DbSet<Item> Items { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<TripItem> TripItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // TripItem → Trip
            modelBuilder.Entity<TripItem>()
                .HasOne(ti => ti.Trip)
                .WithMany(t => t.TripItems)
                .HasForeignKey(ti => ti.TripId)
                .OnDelete(DeleteBehavior.Cascade); // Auto-delete TripItems when Trip is deleted

            // TripItem → Item
            modelBuilder.Entity<TripItem>()
                .HasOne(ti => ti.Item)
                .WithMany(i => i.TripItems)
                .HasForeignKey(ti => ti.ItemId)
                .OnDelete(DeleteBehavior.Restrict);

            // Item → Category
            modelBuilder.Entity<Item>()
                .HasOne(i => i.Category)
                .WithMany(c => c.Items)
                .HasForeignKey(i => i.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // Item → MasterUser
            modelBuilder.Entity<Item>()
                .HasOne(i => i.MasterUser)
                .WithMany(mu => mu.Items)
                .HasForeignKey(i => i.MasterUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Category → MasterUser
            modelBuilder.Entity<Category>()
                .HasOne(c => c.MasterUser)
                .WithMany(mu => mu.Categories)
                .HasForeignKey(c => c.MasterUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Trip → MasterUser
            modelBuilder.Entity<Trip>()
                .HasOne(t => t.MasterUser)
                .WithMany(mu => mu.Trips)
                .HasForeignKey(t => t.MasterUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // AppUserLogin → MasterUser (if needed)
            modelBuilder.Entity<AppUserLogin>()
                .HasOne(l => l.MasterUser)
                .WithMany(u => u.Logins)
                .HasForeignKey(l => l.MasterUserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Seeding
            SeedData.Populate(modelBuilder);
        }
    }


}
