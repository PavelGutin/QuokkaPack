using Microsoft.Extensions.DependencyInjection;
using QuokkaPack.Data;
using QuokkaPack.Shared.Models;

namespace QuokkaPack.ApiTests
{
    public static class TestSeedHelper
    {
        public static async Task<int> SeedCategoryAsync(ApiTestFactory factory)
        {
            using var scope = factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var category = new Category
            {
                Name = "SeededCategory",
                Description = "Seeded for testing",
                IsDefault = false,
                MasterUserId = new Guid("00000000-0000-0000-0000-000000000001"), // Use a fixed GUID for testing
            };

            db.Categories.Add(category);
            await db.SaveChangesAsync();

            return category.Id;
        }
    }
}
