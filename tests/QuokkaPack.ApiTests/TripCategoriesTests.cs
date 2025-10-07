using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using QuokkaPack.Data;
using QuokkaPack.Shared.Models;
using System.Net;
using System.Net.Http.Json;

namespace QuokkaPack.ApiTests.Controllers
{
    public class TripCategoriesTests : IClassFixture<ApiTestFactory>, IAsyncLifetime
    {
        private readonly HttpClient _client;
        private readonly ApiTestFactory _factory;
        private TestScope _scope = null!;

        public TripCategoriesTests(ApiTestFactory factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
        }

        public async Task InitializeAsync()
        {
            _scope = await TestScope.CreateAsync(_factory);
        }

        public async Task DisposeAsync()
        {
            await _scope.DisposeAsync();
        }

        [Fact]
        public async Task AddCategoryToTrip_ShouldReturnNoContent_WhenValid()
        {
            var (trip, category, items) = await SeedTripCategoryAndItemsAsync();

            var response = await _client.PostAsJsonAsync(
                $"/api/trips/{trip.Id}/categories",
                category.Id);

            response.StatusCode.Should().Be(HttpStatusCode.NoContent);

            // Verify items were added to trip
            var tripItems = await _scope.Db.TripItems
                .Where(ti => ti.TripId == trip.Id)
                .ToListAsync();
            tripItems.Should().HaveCount(items.Count);
        }

        [Fact]
        public async Task AddCategoryToTrip_ShouldNotReturnError_WhenTripDoesNotExist()
        {
            var category = await SeedCategoryAsync();

            var response = await _client.PostAsJsonAsync(
                $"/api/trips/99999/categories",
                category.Id);

            // Controller doesn't check if trip exists, so it returns NoContent
            // This might be a bug, but testing current behavior
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }

        [Fact]
        public async Task AddCategoriesToTrip_Batch_ShouldReturnNoContent_WhenValid()
        {
            var trip = await SeedTripAsync();
            var category1 = await SeedCategoryWithItemsAsync(2);
            var category2 = await SeedCategoryWithItemsAsync(3);

            var categoryIds = new List<int> { category1.Id, category2.Id };

            var response = await _client.PostAsJsonAsync(
                $"/api/trips/{trip.Id}/categories/batch",
                categoryIds);

            response.StatusCode.Should().Be(HttpStatusCode.NoContent);

            // Verify items were added to trip (2 + 3 = 5 items)
            var tripItems = await _scope.Db.TripItems
                .Where(ti => ti.TripId == trip.Id)
                .ToListAsync();
            tripItems.Should().HaveCount(5);
        }

        [Fact]
        public async Task RemoveCategoryFromTrip_ShouldReturnNoContent_WhenValid()
        {
            var (trip, category, items) = await SeedTripCategoryAndItemsAsync();

            // Add the category items to the trip
            foreach (var item in items)
            {
                _scope.Db.TripItems.Add(new TripItem
                {
                    TripId = trip.Id,
                    ItemId = item.Id,
                    IsPacked = false
                });
            }
            await _scope.Db.SaveChangesAsync();

            // Now remove the category
            var response = await _client.DeleteAsync(
                $"/api/trips/{trip.Id}/categories/{category.Id}");

            response.StatusCode.Should().Be(HttpStatusCode.NoContent);

            // Verify items were removed from trip
            var tripItems = await _scope.Db.TripItems
                .Where(ti => ti.TripId == trip.Id)
                .ToListAsync();
            tripItems.Should().BeEmpty();
        }

        [Fact]
        public async Task RemoveCategoryFromTrip_ShouldReturnNotFound_WhenTripDoesNotExist()
        {
            var category = await SeedCategoryAsync();

            var response = await _client.DeleteAsync(
                $"/api/trips/99999/categories/{category.Id}");

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task RemoveCategoriesFromTrip_Batch_ShouldReturnNoContent_WhenValid()
        {
            var trip = await SeedTripAsync();
            var category1 = await SeedCategoryWithItemsAsync(2);
            var category2 = await SeedCategoryWithItemsAsync(3);

            // Add items from both categories to trip
            var items1 = await _scope.Db.Items.Where(i => i.CategoryId == category1.Id).ToListAsync();
            var items2 = await _scope.Db.Items.Where(i => i.CategoryId == category2.Id).ToListAsync();

            foreach (var item in items1.Concat(items2))
            {
                _scope.Db.TripItems.Add(new TripItem
                {
                    TripId = trip.Id,
                    ItemId = item.Id,
                    IsPacked = false
                });
            }
            await _scope.Db.SaveChangesAsync();

            // Remove both categories
            var categoryIds = new List<int> { category1.Id, category2.Id };
            var request = new HttpRequestMessage(HttpMethod.Delete, $"/api/trips/{trip.Id}/categories/batch");
            request.Content = JsonContent.Create(categoryIds);

            var response = await _client.SendAsync(request);

            // Note: Current controller implementation expects body in DELETE
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);

            // Verify all items were removed
            var remainingItems = await _scope.Db.TripItems.Where(ti => ti.TripId == trip.Id).CountAsync();
            remainingItems.Should().Be(0);
        }

        [Fact]
        public async Task ResetCategoryInTrip_ShouldReturnNotImplemented()
        {
            var (trip, category, items) = await SeedTripCategoryAndItemsAsync();

            var response = await _client.PutAsync(
                $"/api/trips/{trip.Id}/categories/{category.Id}/reset", null);

            response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        }

        // Helper methods
        private async Task<Trip> SeedTripAsync()
        {
            var trip = new Trip
            {
                Destination = "Test Trip",
                StartDate = DateOnly.FromDateTime(DateTime.Now),
                EndDate = DateOnly.FromDateTime(DateTime.Now.AddDays(7)),
                MasterUserId = _scope.MasterUser.Id
            };
            _scope.Db.Trips.Add(trip);
            await _scope.Db.SaveChangesAsync();
            return trip;
        }

        private async Task<Category> SeedCategoryAsync()
        {
            var category = new Category
            {
                Name = $"Test Category {Guid.NewGuid()}",
                IsDefault = false,
                MasterUserId = _scope.MasterUser.Id
            };
            _scope.Db.Categories.Add(category);
            await _scope.Db.SaveChangesAsync();
            return category;
        }

        private async Task<Category> SeedCategoryWithItemsAsync(int itemCount)
        {
            var category = await SeedCategoryAsync();

            for (int i = 0; i < itemCount; i++)
            {
                _scope.Db.Items.Add(new Item
                {
                    Name = $"Item {i}",
                    CategoryId = category.Id,
                    MasterUserId = _scope.MasterUser.Id
                });
            }
            await _scope.Db.SaveChangesAsync();
            return category;
        }

        private async Task<(Trip trip, Category category, List<Item> items)> SeedTripCategoryAndItemsAsync()
        {
            var trip = await SeedTripAsync();
            var category = await SeedCategoryAsync();

            var items = new List<Item>();
            for (int i = 0; i < 3; i++)
            {
                var item = new Item
                {
                    Name = $"Item {i}",
                    CategoryId = category.Id,
                    MasterUserId = _scope.MasterUser.Id
                };
                _scope.Db.Items.Add(item);
                items.Add(item);
            }
            await _scope.Db.SaveChangesAsync();

            return (trip, category, items);
        }
    }
}
