using Microsoft.Extensions.DependencyInjection;
using QuokkaPack.API.Services;
using QuokkaPack.Data;
using QuokkaPack.Data.Models;
using QuokkaPack.Shared.Models;
using System.Security.Claims;

namespace QuokkaPack.ApiTests
{
    public static class TestSeedHelper
    {

        private static ClaimsPrincipal CreateTestPrincipal()
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.Name, "TestUser"),
                new Claim(ClaimTypes.NameIdentifier, "test-user-id"),
                new Claim("preferred_username", "test@example.com"),
                new Claim("http://schemas.microsoft.com/identity/claims/objectidentifier", "11111111-1111-1111-1111-111111111111"),
                new Claim("iss", "https://login.microsoftonline.com/your-tenant-id/v2.0")
            };

            var identity = new ClaimsIdentity(claims, "TestAuth");
            return new ClaimsPrincipal(identity);
        }

        private static async Task<(AppDbContext db, MasterUser masterUser)> GetDbAndMasterUserAsync(ApiTestFactory factory)
        {
            var scope = factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var userResolver = scope.ServiceProvider.GetRequiredService<IUserResolver>();
            var principal = CreateTestPrincipal();
            var masterUser = await userResolver.GetOrCreateAsync(principal);
            return (db, masterUser);
        }

        private static Category CreateCategory(Guid masterUserId) => new Category
        {
            Name = "SeededCategory",
            Description = "Seeded for testing",
            IsDefault = false,
            MasterUserId = masterUserId
        };

        private static Item CreateItem(Guid masterUserId) => new Item
        {
            Name = "SeededItem",
            Notes = "Seeded for testing",
            IsEssential = false,
            MasterUserId = masterUserId
        };

        private static Trip CreateTrip(Guid masterUserId) => new Trip
        {
            Destination = "SeededTrip",
            StartDate = DateTime.Parse("2025/01/01"),
            EndDate = DateTime.Parse("2025/02/01"),
            MasterUserId = masterUserId
        };

        public static async Task<Category> SeedCategoryAsync(ApiTestFactory factory)
        {
            var (db, masterUser) = await GetDbAndMasterUserAsync(factory);
            var category = CreateCategory(masterUser.Id);
            db.Categories.Add(category);
            await db.SaveChangesAsync();
            return category;
        }

        public static async Task<Item> SeedItemAsync(ApiTestFactory factory)
        {
            var (db, masterUser) = await GetDbAndMasterUserAsync(factory);
            var item = CreateItem(masterUser.Id);
            db.Items.Add(item);
            await db.SaveChangesAsync();
            return item;
        }

        public static async Task<Item> SeedCatgoryAndItemAsync(ApiTestFactory factory)
        {
            var (db, masterUser) = await GetDbAndMasterUserAsync(factory);
            var category = CreateCategory(masterUser.Id);
            var item = CreateItem(masterUser.Id);
            item.Categories.Add(category);
            db.Items.Add(item);
            await db.SaveChangesAsync();
            return item;
        }

        public static async Task<Trip> SeedTripAsync(ApiTestFactory factory)
        {
            var (db, masterUser) = await GetDbAndMasterUserAsync(factory);
            var trip = CreateTrip(masterUser.Id);
            db.Trips.Add(trip);
            await db.SaveChangesAsync();
            return trip;
        }

        //public static async Task<TripItem> SeedTripAndItemAsync(ApiTestFactory factory)
        //{
        //    var (db, masterUser) = await GetDbAndMasterUserAsync(factory);
        //    var trip = CreateTrip(masterUser.Id);
        //    var category = CreateCategory(masterUser.Id);
        //    var item = CreateItem(masterUser.Id);
        //    item.Categories.Add(category);

        //    var tripItem = new TripItem
        //    {
        //        Item = item,
        //        IsPacked = false
        //    };
        //    trip.TripItems.Add(tripItem);
        //    db.Trips.Add(trip);
        //    await db.SaveChangesAsync();
        //    return tripItem;
        //}
    }
}
