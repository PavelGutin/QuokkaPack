using Microsoft.Extensions.DependencyInjection;
using QuokkaPack.API.Services;
using QuokkaPack.Data;
using QuokkaPack.Shared.Models;
using System.Security.Claims;

namespace QuokkaPack.ApiTests
{
    public static class TestSeedHelper
    {

        public static async Task<int> SeedCategoryAsync(ApiTestFactory factory)
        {
            using var scope = factory.Services.CreateScope();

            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var userResolver = scope.ServiceProvider.GetRequiredService<IUserResolver>();

            var claims = new[]
            {
                new Claim(ClaimTypes.Name, "TestUser"),
                new Claim(ClaimTypes.NameIdentifier, "test-user-id"),
                new Claim("preferred_username", "test@example.com"),
                new Claim("http://schemas.microsoft.com/identity/claims/objectidentifier", "11111111-1111-1111-1111-111111111111"),
                new Claim("iss", "https://login.microsoftonline.com/your-tenant-id/v2.0")
            };

            var identity = new ClaimsIdentity(claims, "TestAuth");
            var principal = new ClaimsPrincipal(identity);

            var masterUser = await userResolver.GetOrCreateAsync(principal);

            var category = new Category
            {
                Name = "SeededCategory",
                Description = "Seeded for testing",
                IsDefault = false,
                MasterUserId = masterUser.Id, // Use a fixed GUID for testing
            };

            db.Categories.Add(category);
            await db.SaveChangesAsync();

            return category.Id;
        }

        public static async Task<Item> SeedCatgoryAndItemAsync(ApiTestFactory factory)
        {
            using var scope = factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var userResolver = scope.ServiceProvider.GetRequiredService<IUserResolver>();

            var claims = new[]
            {
                new Claim(ClaimTypes.Name, "TestUser"),
                new Claim(ClaimTypes.NameIdentifier, "test-user-id"),
                new Claim("preferred_username", "test@example.com"),
                new Claim("http://schemas.microsoft.com/identity/claims/objectidentifier", "11111111-1111-1111-1111-111111111111"),
                new Claim("iss", "https://login.microsoftonline.com/your-tenant-id/v2.0")
            };

            var identity = new ClaimsIdentity(claims, "TestAuth");
            var principal = new ClaimsPrincipal(identity);
            var masterUser = await userResolver.GetOrCreateAsync(principal);

            var category = new Category
            {
                Name = "SeededCategory",
                Description = "Seeded for testing",
                IsDefault = false,
                MasterUserId = masterUser.Id, 
            };

            db.Categories.Add(category);

            var categoryId = category.Id;

            var item = new Item
            {
                Name = "SeededItem",
                Notes = "Seeded for testing",
                IsEssential = false,
                MasterUserId = category.MasterUserId, // Use the same MasterUserId as the category
                Categories = new List<Category> { category } // Associate the item with the seeded category
            };
            db.Items.Add(item);
            await db.SaveChangesAsync();

            return item;
        }
    }
}
