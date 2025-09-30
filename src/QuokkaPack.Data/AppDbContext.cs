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

            // Configure delete behavior based on database provider
            var deleteRestrict = Database.IsSqlite() ? DeleteBehavior.Restrict : DeleteBehavior.Restrict;

            // TripItem → Trip
            modelBuilder.Entity<TripItem>()
                .HasOne(ti => ti.Trip)
                .WithMany(t => t.TripItems)
                .HasForeignKey(ti => ti.TripId)
                .OnDelete(deleteRestrict); // Prevent cascade cycles

            // TripItem → Item
            modelBuilder.Entity<TripItem>()
                .HasOne(ti => ti.Item)
                .WithMany(i => i.TripItems)
                .HasForeignKey(ti => ti.ItemId)
                .OnDelete(deleteRestrict);

            // Item → Category
            modelBuilder.Entity<Item>()
                .HasOne(i => i.Category)
                .WithMany(c => c.Items)
                .HasForeignKey(i => i.CategoryId)
                .OnDelete(deleteRestrict);

            // Item → MasterUser
            modelBuilder.Entity<Item>()
                .HasOne(i => i.MasterUser)
                .WithMany(mu => mu.Items)
                .HasForeignKey(i => i.MasterUserId)
                .OnDelete(deleteRestrict);

            // Category → MasterUser
            modelBuilder.Entity<Category>()
                .HasOne(c => c.MasterUser)
                .WithMany(mu => mu.Categories)
                .HasForeignKey(c => c.MasterUserId)
                .OnDelete(deleteRestrict);

            // Trip → MasterUser
            modelBuilder.Entity<Trip>()
                .HasOne(t => t.MasterUser)
                .WithMany(mu => mu.Trips)
                .HasForeignKey(t => t.MasterUserId)
                .OnDelete(deleteRestrict);

            // AppUserLogin → MasterUser (if needed)
            modelBuilder.Entity<AppUserLogin>()
                .HasOne(l => l.MasterUser)
                .WithMany(u => u.Logins)
                .HasForeignKey(l => l.MasterUserId)
                .OnDelete(DeleteBehavior.Cascade);

            // SQLite-specific configurations
            if (Database.IsSqlite())
            {
                // SQLite handles DateTime well natively, but we can add specific configurations if needed
                // For now, we'll rely on the default SQLite DateTime handling
            }

            // Seeding
            SeedData.Populate(modelBuilder);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            
            // Additional configurations can be added here if needed
            // Most SQLite-specific configurations are handled in the DatabaseConfigurationService
        }
    }


}
