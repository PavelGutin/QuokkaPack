using Microsoft.EntityFrameworkCore;

using QuokkaPack.Shared.Models;

namespace QuokkaPack.Data
{
    public static class SeedData
    {
        public static void Populate(ModelBuilder modelBuilder)
        {
            var masterUserId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
            var masterUser = new MasterUser
            {
                Id = masterUserId,
                CreatedAt = new DateTime(2025, 1, 1)
            };

            var categories = new[]
            {
            new Category { Id = 1, Name = "Toiletries", MasterUserId = masterUserId },
            new Category { Id = 2, Name = "Clothing", MasterUserId = masterUserId },
            new Category { Id = 3, Name = "Electronics", MasterUserId = masterUserId },
            new Category { Id = 4, Name = "Outdoor Gear", MasterUserId = masterUserId },
            new Category { Id = 5, Name = "Snacks", MasterUserId = masterUserId },
            new Category { Id = 6, Name = "Documents", MasterUserId = masterUserId }
        };

            var items = new List<Item>();
            int itemId = 1;

            void AddItems(int categoryId, params string[] itemNames)
            {
                items.AddRange(itemNames.Select(name => new Item
                {
                    Id = itemId++,
                    Name = name,
                    CategoryId = categoryId,
                    MasterUserId = masterUserId
                }));
            }

            AddItems(1, "Toothbrush", "Toothpaste", "Shampoo", "Deodorant", "Razor", "Face Wash", "Floss");
            AddItems(2, "T-shirts", "Jeans", "Sweater", "Raincoat", "Socks", "Underwear", "Pajamas", "Hat");
            AddItems(3, "Phone Charger", "Power Bank", "Headphones", "Laptop", "Kindle", "USB Cable");
            AddItems(4, "Hiking Boots", "Tent", "Sleeping Bag", "Flashlight", "Water Bottle", "Backpack");
            AddItems(5, "Granola Bars", "Trail Mix", "Jerky", "Fruit Snacks", "Protein Bars");
            AddItems(6, "Passport", "Boarding Pass", "Travel Insurance", "Itinerary", "ID Card");

            var trips = new[]
            {
            new Trip { Id = 1, Destination = "Tokyo", StartDate = new DateOnly(2025, 4, 10), EndDate = new DateOnly(2025, 4, 24), MasterUserId = masterUserId },
            new Trip { Id = 2, Destination = "Yosemite", StartDate = new DateOnly(2025, 6, 1), EndDate = new DateOnly(2025, 6, 8), MasterUserId = masterUserId },
            new Trip { Id = 3, Destination = "Paris", StartDate = new DateOnly(2025, 7, 15), EndDate = new DateOnly(2025, 7, 30), MasterUserId = masterUserId },
            new Trip { Id = 4, Destination = "Banff", StartDate = new DateOnly(2025, 9, 10), EndDate = new DateOnly(2025, 9, 20), MasterUserId = masterUserId },
            new Trip { Id = 5, Destination = "New York City", StartDate = new DateOnly(2025, 12, 20), EndDate = new DateOnly(2025, 12, 27), MasterUserId = masterUserId },
        };

            var random = new Random();
            var tripItems = new List<TripItem>();
            int tripItemId = 1;

            foreach (var trip in trips)
            {
                // Pick the first 3–5 categories by ID (deterministic)
                var tripCategoryIds = categories
                    .OrderBy(c => c.Id) // predictable order
                    .Skip(trip.Id % 2)  // vary trips a little
                    .Take(3 + (trip.Id % 3)) // 3–5 categories
                    .Select(c => c.Id)
                    .ToList();

                foreach (var catId in tripCategoryIds)
                {
                    var catItems = items.Where(i => i.CategoryId == catId);
                    foreach (var item in catItems)
                    {
                        tripItems.Add(new TripItem
                        {
                            Id = tripItemId++,
                            TripId = trip.Id,
                            ItemId = item.Id,
                            IsPacked = (item.Id + trip.Id) % 2 == 0 // deterministic pattern
                        });
                    }
                }
            }

            modelBuilder.Entity<MasterUser>().HasData(masterUser);
            modelBuilder.Entity<Category>().HasData(categories);
            modelBuilder.Entity<Item>().HasData(items);
            modelBuilder.Entity<Trip>().HasData(trips);
            modelBuilder.Entity<TripItem>().HasData(tripItems);
        }
    }
}
