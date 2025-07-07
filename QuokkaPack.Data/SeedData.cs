using Microsoft.EntityFrameworkCore;
using QuokkaPack.Data.Models;
using QuokkaPack.Shared.Models;

namespace QuokkaPack.Data.Seeding;

public static class SeedData
{
    public static async Task InitializeAsync(AppDbContext context)
    {
        if (await context.MasterUsers.AnyAsync())
            return; // Already seeded

        var masterUserId = Guid.NewGuid();
        var rand = new Random();

        var user = new MasterUser
        {
            Id = masterUserId,
            CreatedAt = DateTime.UtcNow,
            Logins = new List<UserLogin>
            {
                new UserLogin
                {
                    Provider = "entra",
                    ProviderUserId = "3ad08e2d-78a3-404b-8665-bd6fdab60859",
                    Issuer = "https://login.microsoftonline.com/c1532d29-6f77-4b68-a33c-b769515dfd69/v2.0"
                }
            }
        };

        var categoriesSeed = new Dictionary<string, List<string>>
        {
            ["Photography"] = new() { "DSLR Camera", "Tripod", "SD Cards", "Lens Cleaner", "Camera Charger", "Backup Battery" },
            ["Beach"] = new() { "Towel", "Sunscreen", "Beach Tent", "Cooler", "Flip Flops", "Swimsuit", "Beach Chair", "Snorkel Set" },
            ["Hiking"] = new() { "Hiking Boots", "Backpack", "Trail Map", "Water Bottle", "Snacks", "Rain Jacket" },
            ["Camping"] = new() { "Tent", "Sleeping Bag", "Camping Stove", "Lantern", "Bug Spray", "Matches", "First Aid Kit" },
            ["Work Trip"] = new() { "Laptop", "Charger", "Notebook", "Business Cards", "Dress Shoes", "Travel Mouse" }
        };

        var categories = new List<Category>();
        var items = new List<Item>();

        foreach (var (categoryName, itemNames) in categoriesSeed)
        {
            var category = new Category
            {
                Name = categoryName,
                Description = $"Items for {categoryName.ToLower()}",
                IsDefault = true,
                MasterUserId = masterUserId
            };

            foreach (var itemName in itemNames)
            {
                var item = new Item
                {
                    Name = itemName,
                    Notes = "",
                    IsEssential = rand.NextDouble() < 0.5,
                    MasterUserId = masterUserId
                };
                item.Categories.Add(category);
                category.Items.Add(item);
                items.Add(item);
            }

            categories.Add(category);
        }

        var tripDestinations = new[] { "Yellowstone National Park", "Barcelona, Spain", "Tokyo, Japan" };
        var trips = new List<Trip>();
        var today = DateTime.Today;

        for (int i = 0; i < 3; i++)
        {
            var trip = new Trip
            {
                Destination = tripDestinations[i],
                StartDate = today.AddDays((i + 1) * 15),
                EndDate = today.AddDays((i + 1) * 15 + 7),
                MasterUserId = masterUserId,
                Categories = new List<Category>(),
                Item = new List<Item>()
            };

            var selectedCategories = categories.OrderBy(_ => rand.Next()).Take(3).ToList();
            foreach (var cat in selectedCategories)
            {
                trip.Categories.Add(cat);
            }

            foreach (var cat in selectedCategories)
            {
                foreach (var item in cat.Items)
                {
                    if (!trip.Item.Any(i => i.Name == item.Name)) // prevent dupes
                    {
                        trip.Item.Add(item);
                    }
                }
            }

            trips.Add(trip);
        }


        await context.MasterUsers.AddAsync(user);
        await context.Categories.AddRangeAsync(categories);
        await context.Items.AddRangeAsync(items);
        await context.Trips.AddRangeAsync(trips);

        await context.SaveChangesAsync();
    }
}
