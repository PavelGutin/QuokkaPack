using FluentAssertions;

using QuokkaPack.Shared.DTOs.TripItem;
using QuokkaPack.Shared.Models;
using System.Net;
using System.Net.Http.Json;

namespace QuokkaPack.ApiTests.Controllers
{
    public class TripItemsTests : IClassFixture<ApiTestFactory>, IAsyncLifetime
    {
        private readonly HttpClient _client;
        private readonly ApiTestFactory _factory;
        private TestScope _scope = null!;

        public TripItemsTests(ApiTestFactory factory)
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
        public async Task GetAll_ShouldReturnOk() //TODO: why does this fail?????
        {
            var (tripId, tripItemId) = await SeedTripAndItemAsync();
            var response = await _client.GetAsync(BuildTripItemUrl(tripId));
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task GetById_ShouldReturnNotFound_WhenIdDoesNotExist()
        {
            var response = await _client.GetAsync(BuildTripItemUrl(999999));
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Post_ShouldReturnCreated_WhenValid()
        {
            var (tripId, item) = await SeedTripAndItemAsync();

            var tripItemCreateDto = new TripItemCreateDto
            {
                ItemId = item.Id,
                IsPacked = false
            };

            var response = await _client.PostAsJsonAsync(BuildTripItemUrl(tripId), tripItemCreateDto);
            response.StatusCode.Should().Be(HttpStatusCode.Created);
        }

        [Fact]
        public async Task Put_ShouldReturnBadRequest_WhenIdMismatch()
        {
            var tripItem = await SeedTripItemAsync();

            var tripItemEditDto = new TripItemEditDto
            {
                Id = tripItem.Id + 1, // Intentionally mismatching ID
                IsPacked = false
            };

            var response = await _client.PutAsJsonAsync(BuildTripItemUrl(tripItem.Trip.Id, tripItem.Id), tripItemEditDto);
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Put_ShouldReturnNoContent_WhenValid()
        {
            var tripItem = await SeedTripItemAsync();

            var tripItemEditDto = new TripItemEditDto
            {
                Id = tripItem.Id,
                IsPacked = !tripItem.IsPacked 
            };
            var putResponse = await _client.PutAsJsonAsync(BuildTripItemUrl(tripItem.Trip.Id, tripItem.Id), tripItemEditDto);

            putResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }

        [Fact]
        public async Task Delete_ShouldReturnNotFound_WhenIdInvalid()
        {
            var response = await _client.DeleteAsync(BuildTripItemUrl(999999, 999999));
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Delete_ShouldReturnNoContent_WhenValid()
        {
            var tripItem = await SeedTripItemAsync();
            var deleteResponse = await _client.DeleteAsync(BuildTripItemUrl(tripItem.Trip.Id, tripItem.Id));
            deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }

        // Helper methods to seed data and build URLs

        private async Task<(int tripId, Item item)> SeedTripAndItemAsync()
        {
            var trip = BuildTrip(_scope.MasterUser.Id);
            var item = BuildItem(_scope.MasterUser.Id);

            _scope.Db.Trips.Add(trip);
            _scope.Db.Items.Add(item);
            await _scope.Db.SaveChangesAsync();

            return (trip.Id, item);
        }

        private async Task<TripItem> SeedTripItemAsync()
        {
            var trip = BuildTrip(_scope.MasterUser.Id);
            var item = BuildItem(_scope.MasterUser.Id);

            _scope.Db.Trips.Add(trip);
            _scope.Db.Items.Add(item);
            await _scope.Db.SaveChangesAsync();

            var tripItem = BuildTripItem(trip.Id, item.Id);
            _scope.Db.TripItems.Add(tripItem);
            await _scope.Db.SaveChangesAsync();

            return tripItem;
        }

        private string BuildTripItemUrl(int tripId, int itemId)
        {
            return $"api/trips/{tripId}/tripItems/{itemId}";
        }

        private string BuildTripItemUrl(int tripId)
        {
            return $"api/trips/{tripId}/tripItems";
        }

        private Trip BuildTrip(Guid masterUserId)
        {
            return new Trip
            {
                Destination = "Test Trip",
                StartDate = DateOnly.FromDateTime(DateTime.Parse("2025/01/01")),
                EndDate = DateOnly.FromDateTime(DateTime.Parse("2025/02/01")),
                MasterUserId = masterUserId
            };
        }

        private Item BuildItem(Guid masterUserId)
        {
            return new Item
            {
                Name = "Test Item",
                MasterUserId = masterUserId
            };
        }

        private TripItem BuildTripItem(int tripId, int itemId)
        {
            return new TripItem
            {
                TripId = tripId,
                ItemId = itemId,
                IsPacked = false
            };
        }
    }
}
